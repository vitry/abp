using System;
using System.Collections.Generic;
using EasyNetQ.DI;
using Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

namespace Volo.Abp.EasyNetQ;

public class AbpEasyNetQOptions
{
    public AbpEasyNetQOptions()
    {
        Buses = new EasyNetQBuses();
        EventTypeNamePublishConfigurations = new Dictionary<string, PublishConfiguration>();
        EventTypeNameSubscribeConfigurations = new Dictionary<string, SubscriptionConfiguration>();
        EventTypeNameConsumerConfigurations = new Dictionary<string, ConsumerConfiguration>();
    }

    public EasyNetQBuses Buses { get; set; }

    /// <summary>
    /// EasyNetQ Publish Configuration
    /// </summary>
    public Dictionary<string, PublishConfiguration> EventTypeNamePublishConfigurations { get; set; }

    /// <summary>
    /// EasyNetQ Subscription Configuration
    /// </summary>
    public Dictionary<string, SubscriptionConfiguration> EventTypeNameSubscribeConfigurations { get; set; }

    /// <summary>
    /// Raw Consumer Topology Specific Configuration
    /// </summary>
    public Dictionary<string, ConsumerConfiguration> EventTypeNameConsumerConfigurations { get; set; }

    public Action<IServiceRegister> BusServiceRegister { get; private set; } = x => { };

    public void RegisterBus(Action<IServiceRegister> register)
    {
        BusServiceRegister = register;
    }

    public PublishConfiguration? GetPublishConfiguration(Type type)
    {
        EventTypeNamePublishConfigurations.TryGetValue(type?.FullName ?? "", out var config);
        return config;
    }

    public SubscriptionConfiguration? GetSubscribeConfiguration(Type type)
    {
        EventTypeNameSubscribeConfigurations.TryGetValue(type?.FullName ?? "", out var config);
        return config;
    }

    public ConsumerConfiguration? GetConsumerConfiguration(Type type)
    {
        EventTypeNameConsumerConfigurations.TryGetValue(type?.FullName ?? "", out var config);
        return config;
    }
}