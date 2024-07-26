using System.Collections.Generic;
using EasyNetQ;

namespace Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

public class SubscriptionConfiguration
{
    public IList<string>? Topics { get; set; }
    public ushort? PrefetchCount { get; set; }
    public int? Expires { get; set; }
    public string? QueueName { get; set; }

    public void Specify(ISubscriptionConfiguration config)
    {
        if (PrefetchCount.HasValue)
            config.WithPrefetchCount(PrefetchCount.Value);
        if (Topics?.Count > 0)
            foreach (var topic in Topics)
            {
                config.WithTopic(topic);
            }
        if (Expires.HasValue)
            config.WithExpires(Expires.Value);
        if (!string.IsNullOrEmpty(QueueName))
            config.WithQueueName(QueueName);
    }
}