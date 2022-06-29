using System;
using EasyNetQ;

namespace Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

public class PublishConfiguration
{
    public PublishConfiguration(byte? priority, string topic, int? expires)
    {
        Priority = priority;
        Topic = topic;
        Expires = expires;
    }

    public byte? Priority { get; set; }

    public string Topic { get; set; }

    public int? Expires { get; set; }

    public Action<IPublishConfiguration> PublishWith()
    {
        return config =>
        {
            if (Priority.HasValue) config.WithPriority(Priority.Value);
            if (!Topic.IsNullOrEmpty()) config.WithTopic(Topic);
            if (Expires.HasValue) config.WithExpires(Expires.Value);
        };
    }
}