using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.EasyNetQ;
using Volo.Abp.Modularity;

namespace Volo.Abp.EventBus.EasyNetQ;

[DependsOn(typeof(AbpEventBusEasyNetQModule))]
public class AbpEventBusEasyNetQTestModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var builder = new ConfigurationBuilder().AddJsonFile("D:\\github\\abp\\framework\\test\\Volo.Abp.EventBus.EasyNetQ.Test\\Volo\\appsettings.json", optional: true, reloadOnChange: true);
        var config = builder.Build();
        context.Services.ReplaceConfiguration(config);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //context.Services.Configure<AbpEasyNetQOptions>(opt =>
        //{
        //    opt.Buses = new EasyNetQBuses();
        //    opt.Buses.Default = "host=localhost;prefetchcount=50";
        //    opt.Buses.Add("Dev", "host=localhost;prefetchcount=30");
        //    opt.EventTypeNameSubscribeConfigurations.Add("Volo.Abp.EventBus.EasyNetQ.MySimpleEventData", new Abp.EasyNetQ.Volo.Abp.EasyNetQ.SubscribeConfiguration(new List<string> { "mytopic" }, 100, null));
        //});
        //context.Services.Configure<AbpEasyNetQEventBusOptions>(opt =>
        //{
        //    opt.BusName = "Dev";
        //    opt.SubscriptionId = "test";
        //});
    }
}