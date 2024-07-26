using System;
using System.Threading.Tasks;

namespace Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

public interface IEasyNetQSubscriber
{
    void Initialize(string subscriptionId, string? busName = null);

    void Subscribe(Type eventType);

    void UnSubscribe(Type eventType);

    void OnMessageReceived(Func<object, string, Task> callback);
}