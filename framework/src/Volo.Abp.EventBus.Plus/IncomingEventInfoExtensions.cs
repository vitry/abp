using System;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;

namespace Volo.Abp.EventBus.Plus;

public static class IncomingEventInfoExtensions
{
    public static void RecordException(this IncomingEventInfo incomingEventInfo, Exception exception)
    {
        if (incomingEventInfo.IsFailureRetrying())
        {
            incomingEventInfo.AddRetryCount();
        }
        incomingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Failed, true);
        incomingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Exception, exception);
    }

    public static bool TryScheduleRetryLater(this IncomingEventInfo incomignEventInfo, (int retryMaxCount, TimeSpan[] failRetryIntervals) retryConfig, IClock clock)
    {
        int retriedTimes = (int)incomignEventInfo.GetProperty(EventInfoExtraPropertiesConst.Retries);
        // do no retry dead letter
        if (retriedTimes >= retryConfig.retryMaxCount)
        {
            return false;
        }
        incomignEventInfo.NextRetryOn(ScheduleNextRetryTime(retriedTimes, retryConfig.failRetryIntervals));
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

    public static bool IsFailureRetrying(this IncomingEventInfo incomingEventInfo)
    {
        return (bool)incomingEventInfo.GetProperty(EventInfoExtraPropertiesConst.Failed);
    }

    public static void AddRetryCount(this IncomingEventInfo incomingEventInfo)
    {
        int currentRetries = (int)incomingEventInfo.GetProperty(EventInfoExtraPropertiesConst.Retries);
        incomingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Retries, currentRetries + 1);
    }

    public static void FinishHandle(this IncomingEventInfo incomingEventInfo)
    {
        if (incomingEventInfo.IsFailureRetrying())
        {
            incomingEventInfo.AddRetryCount();
        }
        incomingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Processed, true);
        incomingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Failed, false);
    }

    private static void NextRetryOn(this IncomingEventInfo outgoingEventInfo, DateTime nextRetryTime)
    {
        var retries = outgoingEventInfo.GetProperty(EventInfoExtraPropertiesConst.Retries);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.Retries, retries == null ? 0 : (int)retries);
        outgoingEventInfo.SetProperty(EventInfoExtraPropertiesConst.NextRetryTime, nextRetryTime);
    }
}