using System;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;

namespace Volo.Abp.EventBus.Plus;

public static class OutgoingEventInfoExtensions
{
    public static void RecordException(this OutgoingEventInfo outgoingEventInfo, Exception exception)
    {
        if (outgoingEventInfo.IsFailureRetrying())
        {
            outgoingEventInfo.AddRetryCount();
        }
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Failed, true);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Exception, exception);
    }

    public static bool TryScheduleRetryLater(this OutgoingEventInfo outgoingEventInfo, (int retryMaxCount, TimeSpan[] failRetryIntervals) retryConfig, IClock clock)
    {
        int retriedTimes = (int)outgoingEventInfo.GetProperty(EventInfoExtraPropertiesConst.Retries);
        // do not retry dead letter
        if (retriedTimes >= retryConfig.retryMaxCount)
        {
            return false;
        }
        outgoingEventInfo.NextRetryOn(ScheduleNextRetryTime(retriedTimes, retryConfig.failRetryIntervals));
        return true;

        DateTime ScheduleNextRetryTime(int retryCount, TimeSpan[] failRetryIntervals)
        {
            int intervalIndex =
                retryCount > failRetryIntervals.Length - 1
                ? failRetryIntervals.Length : retryCount;

            TimeSpan interval = failRetryIntervals[intervalIndex];
            return clock.Now + interval;
        }
    }

    public static bool IsFailureRetrying(this OutgoingEventInfo outgoingEventInfo)
    {
        return (bool)outgoingEventInfo.GetProperty(EventInfoExtraPropertiesConst.Failed);
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
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Processed, true);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Failed, false);
    }

    private static void NextRetryOn(this OutgoingEventInfo outgoingEventInfo, DateTime nextRetryTime)
    {
        var retries = outgoingEventInfo.GetProperty(EventInfoExtraPropertiesConst.Retries);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Retries, retries == null ? 0 : (int)retries);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.NextRetryTime, nextRetryTime);
    }
}