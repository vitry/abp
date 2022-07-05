using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Json;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace Volo.Abp.EasyNetQ;

[DependsOn(
    typeof(AbpJsonModule),
    typeof(AbpThreadingModule)
)]
public class AbpEasyNetQModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<AbpEasyNetQOptions>(configuration.GetSection("EasyNetQ"));
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        context.ServiceProvider.GetRequiredService<IBusPool>().Dispose();
    }
}