using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Plus;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Volo.Abp.EventBus.Volo.Abp.EventBus.Distributed;

public abstract class EnhancedDistributedEventBusBase : DistributedEventBusBase, IEnhancedDistributedEventBus
{
    protected EnhancedDistributedEventBusBase(
        IServiceScopeFactory serviceScopeFactory, 
        ICurrentTenant currentTenant, 
        IUnitOfWorkManager unitOfWorkManager, 
        IOptions<AbpDistributedEventBusOptions> abpDistributedEventBusOptions, 
        IGuidGenerator guidGenerator, 
        IClock clock, 
        IEventHandlerInvoker eventHandlerInvoker
        ) : base(
            serviceScopeFactory, 
            currentTenant, 
            unitOfWorkManager, 
            abpDistributedEventBusOptions, 
            guidGenerator, 
            clock, 
            eventHandlerInvoker)
    {
    }

    public Task PublishAsync<TEvent>(TEvent eventData, string[] tags, bool onUnitOfWorkComplete = true, bool useOutbox = true) where TEvent : class
    {
        return PublishAsync(typeof(TEvent), eventData, tags, onUnitOfWorkComplete, useOutbox);
    }

    public async Task PublishAsync(Type eventType, object eventData, string[] tags, bool onUnitOfWorkComplete = true, bool useOutbox = true)
    {
        if (onUnitOfWorkComplete && UnitOfWorkManager.Current != null)
        {
            AddToUnitOfWork(
                UnitOfWorkManager.Current,
                new UnitOfWorkEventRecord(eventType, eventData, EventOrderGenerator.GetNext(), useOutbox)
            );
            return;
        }

        if (useOutbox)
        {
            if (await AddToOutboxAsync(eventType, eventData, tags))
            {
                return;
            }
        }

        await PublishToEventBusAsync(eventType, eventData);
    }

    private async Task<bool> AddToOutboxAsync(Type eventType, object eventData, string[] tags)
    {
        var unitOfWork = UnitOfWorkManager.Current;
        if (unitOfWork == null)
        {
            return false;
        }

        foreach (var outboxConfig in AbpDistributedEventBusOptions.Outboxes.Values.OrderBy(x => x.Selector is null))
        {
            if (outboxConfig.Selector == null || outboxConfig.Selector(eventType))
            {
                var eventOutbox =
                    (IEventOutbox)unitOfWork.ServiceProvider.GetRequiredService(outboxConfig.ImplementationType);
                var eventName = EventNameAttribute.GetNameOrDefault(eventType);
                var outgoingEventInfo = new OutgoingEventInfo(
                        GuidGenerator.Create(),
                        eventName,
                        Serialize(eventData),
                        Clock.Now
                    );
                outgoingEventInfo.SetProperty("tags", tags);
                await eventOutbox.EnqueueAsync(outgoingEventInfo);
                return true;
            }
        }

        return false;
    }
}