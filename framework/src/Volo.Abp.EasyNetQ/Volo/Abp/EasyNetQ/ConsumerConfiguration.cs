using EasyNetQ;

namespace Volo.Abp.EasyNetQ;

public class ConsumerConfiguration
{
    public string? QueueName { get; set; }
    public string? ExchangeName { get; set; }
    public string? ExchangeType { get; set; }
    public string? RoutingKey { get; set; }
    public ushort? PrefetchCount { get; set; }

    public void Specify(IConsumerConfiguration config)
    {
        if (PrefetchCount.HasValue)
            config.WithPrefetchCount(PrefetchCount.Value);
    }
}