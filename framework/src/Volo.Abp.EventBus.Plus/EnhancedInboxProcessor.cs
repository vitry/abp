using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Volo.Abp.EventBus.Plus;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IInboxProcessor), typeof(EnhancedInboxProcessor))]
public class EnhancedInboxProcessor : InboxProcessor, IInboxProcessor, ITransientDependency
{
    public EnhancedInboxProcessor(
        IServiceProvider serviceProvider,
        AbpAsyncTimer timer,
        IDistributedEventBus distributedEventBus,
        IAbpDistributedLock distributedLock,
        IUnitOfWorkManager unitOfWorkManager,
        IClock clock,
        IOptions<AbpEventBusBoxesOptions> eventBusBoxesOptions,
        IConsumeProcessLogger consumeProcessLogger)
        : base(serviceProvider, timer, distributedEventBus, distributedLock, unitOfWorkManager, clock, eventBusBoxesOptions)
    {
        ConsumeProcessLogger = consumeProcessLogger;
    }

    protected IConsumeProcessLogger ConsumeProcessLogger { get; }

    protected override async Task RunAsync()
    {
        if (StoppingToken.IsCancellationRequested)
        {
            return;
        }

        await using (var handle = await DistributedLock.TryAcquireAsync(DistributedLockName, cancellationToken: StoppingToken))
        {
            if (handle != null)
            {
                await DeleteOldEventsAsync();

                while (true)
                {
                    var waitingEvents = await Inbox.GetWaitingEventsAsync(EventBusBoxesOptions.InboxWaitingEventMaxCount, StoppingToken);
                    if (waitingEvents.Count <= 0)
                    {
                        break;
                    }

                    Logger.LogInformation($"Found {waitingEvents.Count} events in the inbox.");

                    foreach (var waitingEvent in waitingEvents)
                    {
                        using (var uow = UnitOfWorkManager.Begin(isTransactional: true, requiresNew: true))
                        {
                            try
                            {
                                await DistributedEventBus
                                    .AsSupportsEventBoxes()
                                    .ProcessFromInboxAsync(waitingEvent, InboxConfig);
                                waitingEvent.FinishHandle();
                                await Inbox.MarkAsProcessedAsync(waitingEvent.Id);
                                Logger.LogInformation($"Processed the incoming event with id = {waitingEvent.Id:N}");
                            }
                            catch (Exception ex)
                            {
                                waitingEvent.RecordException(ex);

                                // dead letter wouldn't try again
                                // todo-kai: set retry timing.
                                (int retryMaxCount, TimeSpan[] failRetryIntervals) retryConfig = (10, new[] { TimeSpan.FromMinutes(1) });
                                if (waitingEvent.TryScheduleRetryLater(retryConfig, Clock))
                                {
                                    await Inbox.EnqueueAsync(waitingEvent);
                                    Logger.LogInformation($"Failed consuming the event to the message broker with id = {waitingEvent.Id:N}. Will be retried later on {waitingEvent.ExtraProperties[EventInfoExtraPropertiesConst.NextRetryTime]}.");
                                    continue;
                                }
                                else
                                {
                                    await Inbox.MarkAsProcessedAsync(waitingEvent.Id);
                                    Logger.LogInformation($"Failed consuming the event to the message broker with id = {waitingEvent.Id:N}. No more retry.");
                                }
                            }
                            finally
                            {
                                await ConsumeProcessLogger.Log(waitingEvent);
                                await uow.CompleteAsync();
                                // todo-kai: log here
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.LogDebug("Could not obtain the distributed lock: " + DistributedLockName);
                try
                {
                    await Task.Delay(EventBusBoxesOptions.DistributedLockWaitDuration, StoppingToken);
                }
                catch (TaskCanceledException) { }
            }
        }
    }
}