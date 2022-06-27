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
using Volo.Abp.EventBus.EasyNetQ.Volo.Abp.EventBus.EasyNetQ;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using static Volo.Abp.EventBus.EasyNetQ.EasyNetQConst;

namespace Volo.Abp.EventBus.EasyNetQ;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IDistributedEventBus), typeof(EasyNetQDistributedEventBus))]
public class EasyNetQDistributedEventBus : DistributedEventBusBase, ISingletonDependency
{
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

        HandlerFactories = new ConcurrentDictionary<Type, List<IEventHandlerFactory>>();
        EventTypes = new ConcurrentDictionary<string, Type>();
    }

    public void Initialize()
    {
        Bus = RabbitHutch.CreateBus(AbpEasyNetQEventBusOptions.Connection);
        SubscribeHandlers(AbpDistributedEventBusOptions.Handlers);
    }

    public void ShutDown()
    {
        Bus.Dispose();
    }

    public async override Task ProcessFromInboxAsync(
        IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
    {
        var eventType = EventTypes.GetOrDefault(incomingEvent.EventName);
        if (eventType == null) return;

        var eventData = Serializer.Deserialize(incomingEvent.EventData, eventType);
        var exceptions = new List<Exception>();

        // todo-kai: pipeline handle if required;
        await TriggerHandlersAsync(eventType, eventData, exceptions, inboxConfig);

        if (exceptions.Any())
        {
            ThrowOriginalExceptions(eventType, exceptions);
        }
    }

    public override async Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
    {
        var eventName = outgoingEvent.EventName;
        var eventData = outgoingEvent.EventData;
        var eventType = EventTypes.GetOrDefault(eventName);
        var @event = Serializer.Deserialize(eventData, eventType);

        outgoingEvent.ExtraProperties.TryGetValue(PublishConfigurations.Priority, out var priority);
        outgoingEvent.ExtraProperties.TryGetValue(PublishConfigurations.Topic, out var topic);
        outgoingEvent.ExtraProperties.TryGetValue(PublishConfigurations.Expires, out var expire);

        await PublishAsync(eventType, @event, (byte?)priority, (string)topic, (int?)expire);
    }

    public async Task PublishAsync(
        Type eventType, object eventData,
        byte? priority, string topic, int? expire)
    {
        await Bus.PubSub.PublishAsync(eventData, eventType, config =>
        {
            if (priority.HasValue) config.WithPriority(priority.Value);
            if (!topic.IsNullOrEmpty()) config.WithTopic(topic);
            if (expire.HasValue) config.WithExpires(expire.Value);
        }).ConfigureAwait(false);
    }

    public override async Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig)
    {
        foreach (var outgoingEvent in outgoingEvents)
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

        return new EventHandlerFactoryUnregistrar(this, eventType, factory);
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
        return HandlerFactories
            .Where(hf => ShouldTriggerEventForHandler(eventType, hf.Key))
            .Select(handlerFactory =>
                new EventTypeWithEventHandlerFactories(handlerFactory.Key, handlerFactory.Value))
            .ToArray();
    }

    protected async override Task PublishToEventBusAsync(Type eventType, object eventData)
    {
        await PublishAsync(eventType, eventData, null, null, null);
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