using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Plus;

public interface IConsumeProcessLogger
{
    Task Log(IncomingEventInfo incomingEventInfo);
}