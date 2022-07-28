using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public static class EventRecordExtensions
{
    public static IncomingEventInfo ToExtraPropsIncomingEventInfo(this IncomingEventRecord record)
    {
        IncomingEventInfo incomingEventInfo = record.ToIncomingEventInfo();
        foreach (var prop in record.ExtraProperties)
        {
            incomingEventInfo.SetProperty(prop.Key, prop.Value);
        }
        return incomingEventInfo;
    }
    
    public static void UpdateExtraProperties(this IncomingEventRecord record, ExtraPropertyDictionary extraProperties)
    {
        foreach (var prop in extraProperties)
        {
            record.SetProperty(prop.Key, prop.Value);
        }
    }

    public static OutgoingEventInfo ToExtraPropsOutgoingEventInfo(this OutgoingEventRecord record)
    {
        OutgoingEventInfo outgoingEventInfo = record.ToOutgoingEventInfo();
        foreach (var prop in record.ExtraProperties)
        {
            outgoingEventInfo.SetProperty(prop.Key, prop.Value);
        }
        return outgoingEventInfo;
    }

    public static void UpdateExtraProperties(this OutgoingEventRecord record, ExtraPropertyDictionary extraProperties)
    {
        foreach (var prop in extraProperties)
        {
            record.SetProperty(prop.Key, prop.Value);
        }
    }
}