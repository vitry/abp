using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Plus;

public class PublishProcessLogger : IPublishProcessLogger, ISingletonDependency
{
    public PublishProcessLogger(IDistributedEventBus distributedEventBus)
    {
        DistributedEventBus = distributedEventBus;
    }

    protected IDistributedEventBus DistributedEventBus { get; }

    public virtual async Task Log(OutgoingEventInfo outgoingEventInfo)
    {
        // Sync outgoing event sending log.
        if (outgoingEventInfo.EventName != EventNameAttribute.GetNameOrDefault(typeof(OutgoingEventInfoChangedEvent)))
        {
            await DistributedEventBus.PublishAsync(new OutgoingEventInfoChangedEvent(outgoingEventInfo));
        }
    }
}