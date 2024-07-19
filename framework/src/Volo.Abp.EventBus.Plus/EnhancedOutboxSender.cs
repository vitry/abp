using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;
using Volo.Abp.Timing;

namespace Volo.Abp.EventBus.Plus;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IOutboxSender), typeof(EnhancedOutboxSender))]
public class EnhancedOutboxSender : OutboxSender, IOutboxSender, ITransientDependency
{
    public EnhancedOutboxSender(
        IServiceProvider serviceProvider,
        AbpAsyncTimer timer,
        IDistributedEventBus distributedEventBus,
        IAbpDistributedLock distributedLock,
        IOptions<AbpEventBusBoxesOptions> eventBusBoxesOptions,
        IPublishProcessLogger publishProcessLogger,
        IClock clock)
        : base(serviceProvider, timer, distributedEventBus, distributedLock, eventBusBoxesOptions)
    {
        PublishProcessLogger = publishProcessLogger;
        Clock = clock;
    }

    protected IPublishProcessLogger PublishProcessLogger { get; }
    protected IClock Clock { get; }

    protected override async Task PublishOutgoingMessagesAsync(List<OutgoingEventInfo> waitingEvents)
    {
        foreach (var waitingEvent in waitingEvents)
        {
            try
            {
                await DistributedEventBus.AsSupportsEventBoxes().PublishFromOutboxAsync(waitingEvent, OutboxConfig);
                waitingEvent.FinishHandle();
                await Outbox.DeleteAsync(waitingEvent.Id);
                Logger.LogInformation($"Sent the event to the message broker with id = {waitingEvent.Id:N}");
            }
            catch (Exception ex)
            {
                waitingEvent.RecordException(ex);

                // dead letter wouldn't try again.
                // todo-kai: set retry timing.
                (int retryMaxCount, TimeSpan[] failRetryIntervals) retryConfig = (10, new[] { TimeSpan.FromMinutes(1) });
                if (waitingEvent.TryScheduleRetryLater(retryConfig, Clock))
                {
                    await Outbox.EnqueueAsync(waitingEvent);
                    Logger.LogInformation($"Failed sending the event to the message broker with id = {waitingEvent.Id:N}. Will be retried later on {waitingEvent.ExtraProperties[EventInfoExtraPropertiesConst.NextRetryTime]}.");
                    continue;
                }
                else
                {
                    await Outbox.DeleteAsync(waitingEvent.Id);
                    Logger.LogInformation($"Failed sending the event to the message broker with id = {waitingEvent.Id:N}. No more retry.");
                }
            }
            finally
            {
                await PublishProcessLogger.Log(waitingEvent);
                // todo-kai: log here
            }
        }
    }

    protected override Task PublishOutgoingMessagesInBatchAsync(List<OutgoingEventInfo> waitingEvents)
    {
        throw new NotImplementedException();
    }
}