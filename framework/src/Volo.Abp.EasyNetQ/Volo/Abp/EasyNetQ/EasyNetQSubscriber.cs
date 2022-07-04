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
    private IBus _bus;

    public EasyNetQSubscriber(
        IBusPool busPool,
        IOptions<AbpEasyNetQOptions> options,
        AbpAsyncTimer timer,
        IExceptionNotifier exceptionNotifier,
        ILogger<EasyNetQSubscriber> logger)
    {
        BusPool = busPool;
        Timer = timer;
        ExceptionNotifier = exceptionNotifier;
        _options = options.Value;
        _logger = logger;

        Callbacks = new ConcurrentBag<Func<object, string, Task>>();

        Timer.Period = 5000;
        Timer.RunOnStart = true;
        Timer.Elapsed = Timer_Elapsed;
    }

    protected IBusPool BusPool { get; }
    protected AbpAsyncTimer Timer { get; }
    protected IExceptionNotifier ExceptionNotifier { get; }
    protected string BusName { get; private set; }
    protected ConcurrentBag<Func<object, string, Task>> Callbacks { get; }

    public string SubscriptionId { get; private set; }

    public void Initialize([NotNull]string subscriptionId, string busName = null)
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

    public async Task SubscribeAsync(Type eventType)
    {
        var subscribe = await _bus.PubSub.SubscribeAsync(SubscriptionId, eventType,
            (obj, type, cancelToken) => HandleIncomingMessageAsync(obj, type, cancelToken),
            config => _options.GetSubscribeConfiguration(eventType).Specify(config)
        );
    }

    public Task UnSubscribeAsync(Type eventType)
    {
        throw new NotImplementedException();
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
        if (_bus == null)
        {
            await CreateBusAsync();
        }
    }

    protected virtual async Task CreateBusAsync()
    {
        await DisposeBusAsync();
        try
        {
            _bus = BusPool.Get(BusName);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, LogLevel.Error);
            await ExceptionNotifier.NotifyAsync(ex, logLevel: LogLevel.Error);
        }
    }
    
    protected virtual async Task DisposeBusAsync()
    {
        if (_bus == null) return;
        try
        {
            _bus.Dispose();
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
}