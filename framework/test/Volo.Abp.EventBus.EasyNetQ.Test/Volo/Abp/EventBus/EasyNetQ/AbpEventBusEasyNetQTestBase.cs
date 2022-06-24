using Volo.Abp.EventBus.Distributed;
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