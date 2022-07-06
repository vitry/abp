using EasyNetQ;

namespace Volo.Abp.EasyNetQ;

public class ExchangeNameConfigurableConventions : Conventions
{
    public ExchangeNameConfigurableConventions(ITypeNameSerializer typeNameSerializer, AbpEasyNetQOptions options)
        : base(typeNameSerializer)
    {
        ExchangeNamingConvention = (type) =>
        {
            options.EventTypeNameSubscribeConfigurations.TryGetValue(type.FullName, out var config);
            return string.IsNullOrEmpty(config.ExchangeName) ? ExchangeNamingConvention(type) : config.ExchangeName;
        };
    }
}