using System;
using EasyNetQ;

namespace Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

public class PublishConfiguration
{
    public string? Topic { get; set; }

    public int? Expires { get; set; }

    public void Specify(IPublishConfiguration config)
    {
        if (!Topic.IsNullOrEmpty()) config.WithTopic(Topic);
        if (Expires.HasValue) config.WithExpires(Expires.Value);
    }
}