using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Plus;

public interface IEnhancedDistributedEventHandler<in TEvent> : IDistributedEventHandler<TEvent>
{
    Task TagEventAsync(TEvent eventData);
}
