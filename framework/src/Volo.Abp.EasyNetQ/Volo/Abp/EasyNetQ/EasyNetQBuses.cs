using System;
using System.Collections.Generic;

namespace Volo.Abp.EasyNetQ;

[Serializable]
public class EasyNetQBuses : Dictionary<string, EasyNetQSetting>
{
    public const string DefaultBusName = "Default";

    public EasyNetQSetting Default {
        get => this[DefaultBusName];
        set => this[DefaultBusName] = Check.NotNull(value, nameof(value));
    }

    public EasyNetQSetting GetOrDefault(string busName)
    {
        if (TryGetValue(busName, out var setting))
        {
            return setting;
        }
        return Default;
    }
}

public class EasyNetQSetting
{
    public string Connection { get; set; }

    public string SubscriptionId { get; set; }
}