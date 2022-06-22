using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using static Volo.Abp.EventBus.EasyNetQ.Volo.Abp.EventBus.EasyNetQ.EasyNetQConst;

namespace Volo.Abp.EventBus.EasyNetQ.Volo.Abp.EventBus.EasyNetQ;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IDistributedEventBus), typeof(EasyNetQDistributedEventBus))]
public class EasyNetQDistributedEventBus : DistributedEventBusBase, ISingletonDependency
{
    private readonly ConcurrentDictionary<Type, List<IEventHandlerFactory>> _handlerFactories;

    protected AbpEasyNetQEventBusOptions AbpEasyNetQEventBusOptions { get; }
    protected IEasyNetQSerializer Serializer { get; }
    protected ConcurrentDictionary<Type, List<IEventHandlerFactory>> HandlerFactories { get; }
    protected ConcurrentDictionary<string, Type> EventTypes { get; }
    protected IBus Bus { get; private set; }

    public EasyNetQDistributedEventBus(
        IOptions<AbpEasyNetQEventBusOptions> options,
        IEasyNetQSerializer serializer,
        IServiceScopeFactory serviceScopeFactory,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        IOptions<AbpDistributedEventBusOptions> abpDistributedEventBusOptions,
        IGuidGenerator guidGenerator,
        IClock clock,
        IEventHandlerInvoker eventHandlerInvoker)
        : base(
            serviceScopeFactory,
            currentTenant,
            unitOfWorkManager,
            abpDistributedEventBusOptions,
            guidGenerator,
            clock,
            eventHandlerInvoker)
    {
        AbpEasyNetQEventBusOptions = options.Value;
        Serializer = serializer;
    }

    public void Initialize()
    {
        Bus = RabbitHutch.CreateBus(AbpEasyNetQEventBusOptions.Connection);
        SubscribeHandlers(AbpDistributedEventBusOptions.Handlers);
    }

    public override Task ProcessFromInboxAsync(
        IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
    {
        throw new NotImplementedException();
    }

    public override async Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
    {
        string eventName = outgoingEvent.EventName;
        byte[] eventData = outgoingEvent.EventData;
        Type eventType = EventTypes.GetOrDefault(eventName);

        object @event = Serializer.Deserialize(eventData, eventType);

        await Bus.PubSub.PublishAsync(@event, config =>
        {
            if (outgoingEvent.ExtraProperties.TryGetValue(PublishConfigurations.Priority, out var priority))
                config.WithPriority((byte)priority);
            if (outgoingEvent.ExtraProperties.TryGetValue(PublishConfigurations.Topic, out var topic))
                config.WithTopic((string)priority);
            if (outgoingEvent.ExtraProperties.TryGetValue(PublishConfigurations.Expires, out var expire))
                config.WithExpires((int)expire);
        }).ConfigureAwait(false);
    }

    public override async Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig)
    {
        foreach (OutgoingEventInfo outgoingEvent in outgoingEvents)
        {
            await PublishFromOutboxAsync(@outgoingEvent, outboxConfig).ConfigureAwait(false);
        }
    }

    public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory)
    {
        var handlerFactories = GetOrCreateHandlerFactories(eventType);
        if (factory.IsInFactories(handlerFactories))
        {
            return NullDisposable.Instance;
        }

        handlerFactories.Add(factory);

        var subscribe = Bus.PubSub.SubscribeAsync(AbpEasyNetQEventBusOptions.ConsumerId, eventType,
            (obj, type, cancelToken) =>
            {
                // todo-kai: pipeline handle if required;
                return TriggerHandlersAsync(eventType, obj);
            },
            config =>
            {
                /* todo-kai: config subscribe if required */
            });

        return subscribe.As<IDisposable>();
    }

    public override void Unsubscribe<TEvent>(Func<TEvent, Task> action)
    {
        Check.NotNull(action, nameof(action));

        GetOrCreateHandlerFactories(typeof(TEvent))
            .Locking(factories =>
            {
                factories.RemoveAll(
                    factory =>
                    {
                        var singleInstanceFactory = factory as SingleInstanceHandlerFactory;
                        if (singleInstanceFactory == null)
                        {
                            return false;
                        }

                        var actionHandler = singleInstanceFactory.HandlerInstance as ActionEventHandler<TEvent>;
                        if (actionHandler == null)
                        {
                            return false;
                        }

                        return actionHandler.Action == action;
                    });
            });
    }

    public override void Unsubscribe(Type eventType, IEventHandler handler)
    {
        GetOrCreateHandlerFactories(eventType)
            .Locking(factories =>
            {
                factories.RemoveAll(
                    factory =>
                        factory is SingleInstanceHandlerFactory handlerFactory &&
                        handlerFactory.HandlerInstance == handler
                );
            });
    }

    public override void Unsubscribe(Type eventType, IEventHandlerFactory factory)
    {
        GetOrCreateHandlerFactories(eventType)
            .Locking(factories => factories.Remove(factory));
    }

    public override void UnsubscribeAll(Type eventType)
    {
        GetOrCreateHandlerFactories(eventType)
            .Locking(factories => factories.Clear());
    }

    protected override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord)
    {
        unitOfWork.AddOrReplaceDistributedEvent(eventRecord);
    }

    protected override IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType)
    {
        return _handlerFactories
            .Where(hf => ShouldTriggerEventForHandler(eventType, hf.Key))
            .Select(handlerFactory => 
                new EventTypeWithEventHandlerFactories(handlerFactory.Key, handlerFactory.Value))
            .ToArray();
    }

    protected override Task PublishToEventBusAsync(Type eventType, object eventData)
    {
        throw new NotImplementedException();
    }

    protected override byte[] Serialize(object eventData)
    {
        return Serializer.Serialize(eventData);
    }

    private List<IEventHandlerFactory> GetOrCreateHandlerFactories(Type eventType)
    {
        return HandlerFactories.GetOrAdd(
            eventType,
            type =>
            {
                var eventName = EventNameAttribute.GetNameOrDefault(type);
                EventTypes[eventName] = type;
                return new List<IEventHandlerFactory>();
            }
        );
    }
    private static bool ShouldTriggerEventForHandler(Type targetEventType, Type handlerEventType)
    {
        return handlerEventType == targetEventType || handlerEventType.IsAssignableFrom(targetEventType);
    }
}