using System;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Plus;

public static class OutgoingEventInfoExtensions
{
    public static void HandleOnSchedule(this OutgoingEventInfo outgoingEventInfo)
    {
        outgoingEventInfo.SetStatus(EventInfoStatusConst.Scheduled);
    }

    public static void RecordException(this OutgoingEventInfo outgoingEventInfo, Exception exception)
    {
        outgoingEventInfo.SetStatus(EventInfoStatusConst.Failed);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Exception, exception);
    }

    public static bool TryScheduleRetryLater(this OutgoingEventInfo outgoingEventInfo, (int retryMaxCount, TimeSpan[] failRetryIntervals) retryConfig)
    {
        int retriedTimes = (int)outgoingEventInfo.GetProperty(EventInfoExtraPropertiesConst.Retries);
        // do not retry dead letter
        if (retriedTimes >= retryConfig.retryMaxCount)
        {
            return false;
        }

        outgoingEventInfo.NextRetryOn(ScheduleNextRetryTime(retriedTimes, retryConfig.failRetryIntervals));
        return true;
    }

    public static void FinishHandle(this OutgoingEventInfo outgoingEventInfo)
    {
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Status, EventInfoStatusConst.Succeed);
    }

    private static void NextRetryOn(this OutgoingEventInfo outgoingEventInfo, DateTime nextRetryTime)
    {
        var retries = outgoingEventInfo.GetProperty(EventInfoExtraPropertiesConst.Retries);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Retries, retries == null ? 0 : (int)retries);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.NextRetryTime, nextRetryTime);
    }

    private static void SetStatus(this OutgoingEventInfo outgoingEventInfo, string statusName)
    {
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Status, statusName);
    }

    private static DateTime ScheduleNextRetryTime(int retryCount, TimeSpan[] failRetryIntervals)
    {
        int intervalIndex = 
            retryCount > failRetryIntervals.Length - 1 
            ? failRetryIntervals.Length : retryCount;

        TimeSpan interval = failRetryIntervals[intervalIndex];
        return DateTime.UtcNow + interval;
    }
}