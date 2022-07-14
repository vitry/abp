using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Boxes;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;

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
        IOptions<AbpEventBusBoxesOptions> eventBusBoxesOptions)
        : base(serviceProvider, timer, distributedEventBus, distributedLock, eventBusBoxesOptions)
    {
    }

    protected override async Task PublishOutgoingMessagesAsync(List<OutgoingEventInfo> waitingEvents)
    {
        foreach (var waitingEvent in waitingEvents)
        {
            try
            {
                await DistributedEventBus.AsSupportsEventBoxes().PublishFromOutboxAsync(waitingEvent, OutboxConfig);
            }
            catch (Exception ex)
            {
                waitingEvent.SetProperty("exception", ex);
                waitingEvent.SetProperty("next-retry-time", DateTime.Now.AddMinutes(1));
                await Outbox.EnqueueAsync(waitingEvent);
                return;
            }

            await Outbox.DeleteAsync(waitingEvent.Id);
            Logger.LogInformation($"Sent the event to the message broker with id = {waitingEvent.Id:N}");
        }
    }

    protected override Task PublishOutgoingMessagesInBatchAsync(List<OutgoingEventInfo> waitingEvents)
    {
        // todo-kai-refactor: in batch handle.
        return base.PublishOutgoingMessagesInBatchAsync(waitingEvents);
    }
}