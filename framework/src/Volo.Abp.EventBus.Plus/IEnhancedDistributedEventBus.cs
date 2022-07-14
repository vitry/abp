using System;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Plus;

public interface IEnhancedDistributedEventBus : IDistributedEventBus
{
    Task PublishAsync<TEvent>(
        TEvent eventData,
        string[] tags,
        bool onUnitOfWorkComplete = true,
        bool useOutbox = true)
        where TEvent : class;

    Task PublishAsync(
        Type eventType,
        object eventData,
        string[] tags,
        bool onUnitOfWorkComplete = true,
        bool useOutbox = true);
}
