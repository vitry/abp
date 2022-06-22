using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EasyNetQ;
using Volo.Abp.EventBus.EasyNetQ.Volo.Abp.EventBus.EasyNetQ;
using Volo.Abp.Modularity;

namespace Volo.Abp.EventBus.EasyNetQ;

[DependsOn(
    typeof(AbpEventBusModule),
    typeof(AbpEasyNetQModule))]
public class AbpEventBusEasyNetQModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AbpEasyNetQEventBusOptions>(configuration.GetSection("EasyNetQ:EventBus"));
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        context
            .ServiceProvider
            .GetRequiredService<EasyNetQDistributedEventBus>()
            .Initialize();
    }
}