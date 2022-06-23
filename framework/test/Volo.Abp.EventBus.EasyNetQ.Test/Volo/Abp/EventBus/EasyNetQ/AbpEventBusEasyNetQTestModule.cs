using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EventBus.EasyNetQ.Volo.Abp.EventBus.EasyNetQ;
using Volo.Abp.Modularity;

namespace Volo.Abp.EventBus.EasyNetQ.Test.Volo.Abp.EventBus.EasyNetQ;

[DependsOn(typeof(AbpEventBusEasyNetQModule))]
public class AbpEventBusEasyNetQTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Configure<AbpEasyNetQEventBusOptions>(opt =>
        {
            opt.ConsumerId = "test";
            opt.Connection = "host=localhost;";
        });
    }
}