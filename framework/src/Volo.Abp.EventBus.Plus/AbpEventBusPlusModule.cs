using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Volo.Abp.EventBus.Plus
{
    [DependsOn(
        typeof(AbpEventBusModule)
        )]
    public class AbpEventBusPlusModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();

            Configure<AbpEventBusPlusOptions>(configuration.GetSection("EventBusPlus"));
        }
    }
}