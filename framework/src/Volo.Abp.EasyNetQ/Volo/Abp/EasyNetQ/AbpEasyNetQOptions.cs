using System;
using System.Collections.Generic;
using Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

namespace Volo.Abp.EasyNetQ;

public class AbpEasyNetQOptions
{
    public AbpEasyNetQOptions()
    {
        Buses = new EasyNetQBuses();
    }

    public EasyNetQBuses Buses { get; set; }

    public Dictionary<string, SubscribeConfiguration> EventTypeNameSubscribeConfigurations { get; set; }

    public Dictionary<string, PublishConfiguration> EventTypeNamePublishConfigurations { get; set; }

    public SubscribeConfiguration GetSubscribeConfiguration(Type type)
    {
        EventTypeNameSubscribeConfigurations.TryGetValue(type.FullName, out var config);
        return config;
    }

    public PublishConfiguration GetPublishConfiguration(Type type)
    {
        EventTypeNamePublishConfigurations.TryGetValue(type.FullName, out var config);
        return config;
    }
}
