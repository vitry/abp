using System;
using System.Threading.Tasks;

namespace Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

public interface IEasyNetQSubscriber
{
    void Initialize(string subscriptionId, string busName = null);

    Task SubscribeAsync(Type eventType);

    Task UnSubscribeAsync(Type eventType);

    void OnMessageReceived(Func<object, string, Task> callback);
}