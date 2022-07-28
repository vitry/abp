using System;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Plus;

public static class OutgoingEventInfoExtensions
{
    public static void RecordException(this OutgoingEventInfo outgoingEventInfo, Exception exception)
    {
        if (outgoingEventInfo.IsFailureRetrying())
        {
            outgoingEventInfo.AddRetryCount();
        }
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

    public static bool IsFailureRetrying(this OutgoingEventInfo outgoingEventInfo)
    {
        return outgoingEventInfo.GetProperty(EventInfoExtraPropertiesConst.NextRetryTime) != null;
    }

    public static void AddRetryCount(this OutgoingEventInfo outgoingEventInfo)
    {
        int currentRetries = (int)outgoingEventInfo.GetProperty(EventInfoExtraPropertiesConst.Retries);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Retries, currentRetries + 1);
    }

    public static void FinishHandle(this OutgoingEventInfo outgoingEventInfo)
    {
        if (outgoingEventInfo.IsFailureRetrying())
        {
            outgoingEventInfo.AddRetryCount();
        }
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