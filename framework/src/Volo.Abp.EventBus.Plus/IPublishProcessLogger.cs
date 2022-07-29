using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Plus;

public interface IPublishProcessLogger
{
    Task Log(OutgoingEventInfo outgoingEventInfo);
}