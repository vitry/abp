using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.EasyNetQ.Test.Volo.Abp.EventBus.EasyNetQ;
using Volo.Abp.EventBus.EasyNetQ.Volo.Abp.EventBus.EasyNetQ;
using Volo.Abp.Testing;

namespace Volo.Abp.EventBus.EasyNetQ;

public abstract class AbpEventBusEasyNetQTestBase : AbpIntegratedTest<AbpEventBusEasyNetQTestModule>
{
    protected IDistributedEventBus DistributedEventBus;

    protected AbpEventBusEasyNetQTestBase()
    {
        DistributedEventBus = GetRequiredService<EasyNetQDistributedEventBus>();
    }

    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }
}