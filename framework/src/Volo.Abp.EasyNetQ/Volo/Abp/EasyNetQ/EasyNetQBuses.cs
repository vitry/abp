using System;
using System.Collections.Generic;

namespace Volo.Abp.EasyNetQ;

[Serializable]
public class EasyNetQBuses : Dictionary<string, string>
{
    public const string DefaultBusName = "Default";

    public string Default {
        get => this[DefaultBusName];
        set => this[DefaultBusName] = Check.NotNull(value, nameof(value));
    }

    public string GetOrDefault(string busName)
    {
        if (TryGetValue(busName, out var connection))
        {
            return connection;
        }
        return Default;
    }
}
