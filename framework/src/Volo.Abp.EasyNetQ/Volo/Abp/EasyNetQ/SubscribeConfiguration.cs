using System;
using System.Collections.Generic;
using EasyNetQ;

namespace Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

public class SubscribeConfiguration
{
    public SubscribeConfiguration(IList<string> topics, ushort? prefetchCount, int? expires)
    {
        Topics = topics ?? new List<string>();
        PrefetchCount = prefetchCount;
        Expires = expires;
    }

    public IList<string> Topics { get; set; }
    public ushort? PrefetchCount { get; set; }
    public int? Expires { get; set; }

    public Action<ISubscriptionConfiguration> SubscribeWith()
    {
        return config =>
        {
            if (PrefetchCount.HasValue)
                config.WithPrefetchCount(PrefetchCount.Value);
            if (Topics.Count > 0)
                foreach (var topic in Topics)
                {
                    config.WithTopic(topic);
                }
            if (Expires.HasValue)
                config.WithExpires(Expires.Value);
        };
    }
}