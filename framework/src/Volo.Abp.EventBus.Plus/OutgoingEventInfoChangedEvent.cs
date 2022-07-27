using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Plus;

public class OutgoingEventInfoChangedEvent
{
    public OutgoingEventInfoChangedEvent(OutgoingEventInfo outgoingEventInfo)
    {

    }

    public string EventId { get; private set; }
    public string EventName { get; private set; }
    public string BusProvider { get; private set; }
    public string ProviderInfo { get; private set; }
    public string EventData { get; private set; }
    public string LogInfo { get; private set; }
}
