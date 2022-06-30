using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EasyNetQ;
using Volo.Abp.Modularity;

namespace Volo.Abp.EventBus.EasyNetQ;

[DependsOn(typeof(AbpEventBusEasyNetQModule))]
public class AbpEventBusEasyNetQTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Configure<AbpEasyNetQOptions>(opt =>
        {
            opt.Buses = new EasyNetQBuses();
            opt.Buses.Default = "host=localhost;prefetchcount=50";
        });
        context.Services.Configure<AbpEasyNetQEventBusOptions>(opt =>
        {
            opt.BusName = "Default";
            opt.SubscriptionId = "test";
        });
    }
}