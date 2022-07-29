using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Plus;

public class ConsumeProcessLogger : IConsumeProcessLogger, ISingletonDependency
{
    public ConsumeProcessLogger(IDistributedEventBus distributedEventBus)
    {
        DistributedEventBus = distributedEventBus;
    }

    protected IDistributedEventBus DistributedEventBus { get; }

    public virtual async Task Log(IncomingEventInfo incomingEventInfo)
    {
        if (incomingEventInfo.EventName != EventNameAttribute.GetNameOrDefault(typeof(IncomingEventInfoChangedEvent)))
        {
            await DistributedEventBus.PublishAsync(new IncomingEventInfoChangedEvent(incomingEventInfo));
        }
    }
}