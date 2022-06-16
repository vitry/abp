using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;
using Volo.Abp.Modularity;

namespace Volo.Abp.EasyNetQ;

public class AbpEasyNetQModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<AbpEasyNetQOptions>(configuration.GetSection("EasyNetQ"));
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        context.ServiceProvider.GetRequiredService<IBus>().Dispose();
    }
}