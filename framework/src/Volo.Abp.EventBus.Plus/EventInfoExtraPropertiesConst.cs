namespace Volo.Abp.EventBus.Plus;

public static class EventInfoExtraPropertiesConst
{
    /// <summary>
    /// Retry times
    /// </summary>
    public const string Retries = "Retries";

    public const string Exception = "Exception";

    public const string NextRetryTime = "NextRetryTime";

    public const string Status = "Status";
}

public static class EventInfoStatusConst
{
    public const string Failed = "Failed";

    public const string Succeed = "Succeed";

    public const string Scheduled = "Scheduled";
}
