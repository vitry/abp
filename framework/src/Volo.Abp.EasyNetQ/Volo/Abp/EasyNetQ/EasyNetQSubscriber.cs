using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Threading;

namespace Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

public class EasyNetQSubscriber : IEasyNetQSubscriber, ISingletonDependency, IDisposable
{
    private readonly ILogger<EasyNetQSubscriber> _logger;
    private readonly AbpEasyNetQOptions _options;

    public EasyNetQSubscriber(
        IBusPool busPool,
        IOptions<AbpEasyNetQOptions> options,
        AbpAsyncTimer timer,
        IEasyNetQSerializer serializer,
        IExceptionNotifier exceptionNotifier,
        ILogger<EasyNetQSubscriber> logger)
    {
        BusPool = busPool;
        Timer = timer;
        Serializer = serializer;
        ExceptionNotifier = exceptionNotifier;
        _options = options.Value;
        _logger = logger;

        SubscribeCommandQueue = new BlockingCollection<QueueSubscribeCommand>();
        Subscriptions = new ConcurrentDictionary<Type, IDisposable>();
        Callbacks = new ConcurrentBag<Func<object, string, Task>>();

        Timer.Period = 5000;
        Timer.RunOnStart = true;
        Timer.Elapsed = Timer_Elapsed;
    }

    protected IBusPool BusPool { get; }
    protected IBus? Bus { get; private set; }
    protected AbpAsyncTimer Timer { get; }
    protected IEasyNetQSerializer Serializer { get; }
    protected IExceptionNotifier ExceptionNotifier { get; }
    protected string? BusName { get; private set; }
    protected ConcurrentBag<Func<object, string, Task>> Callbacks { get; }
    protected BlockingCollection<QueueSubscribeCommand> SubscribeCommandQueue { get; }
    protected ConcurrentDictionary<Type, IDisposable> Subscriptions { get; private set; }

    public string? SubscriptionId { get; private set; }

    public void Initialize([NotNull] string subscriptionId, string? busName = null)
    {
        Check.NotNullOrEmpty(subscriptionId, nameof(subscriptionId));
        SubscriptionId = subscriptionId;
        BusName = busName;
        Timer.Start();
    }

    public void OnMessageReceived(Func<object, string, Task> callback)
    {
        Callbacks.Add(callback);
    }

    public void Subscribe(Type eventType)
    {
        SubscribeCommandQueue.TryAdd(new QueueSubscribeCommand(QueueSubscribeType.Subscribe, eventType));
    }

    public void UnSubscribe(Type eventType)
    {
        SubscribeCommandQueue.TryAdd(new QueueSubscribeCommand(QueueSubscribeType.Unsubscribe, eventType));
    }

    protected virtual async Task StartSendQueueSubscribeCommandsAsync()
    {
        if (Bus == null)
        {
            throw new ArgumentNullException(nameof(Bus));
        }
        if (string.IsNullOrEmpty(SubscriptionId))
        {
            throw new ArgumentNullException(nameof(SubscriptionId));
        }

        try
        {
            while (SubscribeCommandQueue.TryTake(out var command))
            {
                switch (command.Type)
                {
                    case QueueSubscribeType.Subscribe:
                        IDisposable? subscription = null;
                        ConsumerConfiguration? config = _options.GetConsumerConfiguration(command.EventType);
                        if (config != null)
                        {
                            var exchage = Bus.Advanced.ExchangeDeclare(config.ExchangeName, config.ExchangeType);
                            var queue = Bus.Advanced.QueueDeclare(config.QueueName);
                            var binding = Bus.Advanced.Bind(exchage, queue, config.RoutingKey);
                            subscription = Bus.Advanced.Consume(queue,
                                (body, _, _, cancelToken) => HandleIncomingMessageAsync(body, command.EventType, cancelToken),
                                c => config.Specify(c));
                        }
                        else
                        {
                            subscription = await Bus.PubSub.SubscribeAsync(SubscriptionId!, command.EventType,
                                (obj, type, cancelToken) => HandleIncomingMessageAsync(obj, type, cancelToken),
                                c => _options.GetSubscribeConfiguration(command.EventType)?.Specify(c)
                            );
                        }
                        Subscriptions.TryAdd(command.EventType, subscription);
                        break;

                    case QueueSubscribeType.Unsubscribe:
                        if (Subscriptions.TryGetValue(command.EventType, out subscription))
                        {
                            subscription.Dispose();
                        }
                        break;

                    default:
                        throw new AbpException($"Unknown {nameof(QueueSubscribeType)}: {command.Type}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, LogLevel.Warning);
            await ExceptionNotifier.NotifyAsync(ex, logLevel: LogLevel.Warning);
        }
    }

    protected virtual async Task HandleIncomingMessageAsync(byte[] body, Type eventType, CancellationToken cancellationToken)
    {
        var @event = Serializer.Deserialize(body, eventType);
        await HandleIncomingMessageAsync(@event, eventType, cancellationToken);
    }

    protected virtual async Task HandleIncomingMessageAsync(object eventData, Type eventType, CancellationToken cancellationToken)
    {
        try
        {
            var eventName = EventNameAttribute.GetNameOrDefault(eventType);

            // todo-kai: retry times
            // todo-kai: pipeline handle if required;
            foreach (var callback in Callbacks)
            {
                await callback(eventData, eventName);
            }
        }
        catch (Exception ex)
        {
            try
            {
            }
            catch
            {
            }

            _logger.LogException(ex);
            await ExceptionNotifier.NotifyAsync(ex);
        }
    }

    protected virtual async Task Timer_Elapsed(AbpAsyncTimer timer)
    {
        if (Bus == null)
        {
            await CreateBusAsync();
            await StartSendQueueSubscribeCommandsAsync();
        }
    }

    protected virtual async Task CreateBusAsync()
    {
        await DisposeBusAsync();
        try
        {
            Bus = BusPool.Get(BusName);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, LogLevel.Error);
            await ExceptionNotifier.NotifyAsync(ex, logLevel: LogLevel.Error);
        }
    }

    protected virtual async Task DisposeBusAsync()
    {
        if (Bus == null) return;

        try
        {
            Bus.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, LogLevel.Warning);
            await ExceptionNotifier.NotifyAsync(ex, logLevel: LogLevel.Warning);
        }
    }

    public void Dispose()
    {
        Timer.Stop();
        AsyncHelper.RunSync(() => DisposeBusAsync());
    }

    protected class QueueSubscribeCommand
    {
        public QueueSubscribeType Type { get; }

        public Type EventType { get; }

        public QueueSubscribeCommand(QueueSubscribeType type, Type eventType)
        {
            Type = type;
            EventType = eventType;
        }
    }

    protected enum QueueSubscribeType
    {
        Subscribe,
        Unsubscribe
    }
}