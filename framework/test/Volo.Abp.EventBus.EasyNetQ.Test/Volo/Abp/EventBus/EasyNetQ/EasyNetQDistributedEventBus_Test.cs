using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Xunit;
using Xunit.Abstractions;

namespace Volo.Abp.EventBus.EasyNetQ;

public class EasyNetQDistributedEventBus_Test : AbpEventBusEasyNetQTestBase
{
    private readonly ITestOutputHelper _output;

    public EasyNetQDistributedEventBus_Test(ITestOutputHelper output)
    {
        this._output = output;
    }

    [Fact]
    public async Task Should_Call_Handler_AndDispose()
    {
        DistributedEventBus.Subscribe<MySimpleEventData, MySimpleDistributedTransientEventHandler>();

        try
        {
            foreach (var data in Enumerable.Range(1, 10000))
            {
                await DistributedEventBus.PublishAsync(new MySimpleEventData(data));
            } 
        }
        catch (Exception ex)
        {
            throw;
        }

        while (MySimpleDistributedTransientEventHandler.HandleCount < 10000)
        {
            _output.WriteLine(MySimpleDistributedTransientEventHandler.HandleCount.ToString());
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
        Assert.Equal(10000, MySimpleDistributedTransientEventHandler.HandleCount);
        Assert.Equal(10000, MySimpleDistributedTransientEventHandler.DisposeCount);
    }

    //[Fact]
    //public async Task Should_Change_TenantId_If_EventData_Is_MultiTenant()
    //{
    //    var tenantId = Guid.NewGuid();

    //    DistributedEventBus.Subscribe<MySimpleEventData>(GetRequiredService<MySimpleDistributedSingleInstanceEventHandler>());

    //    await DistributedEventBus.PublishAsync(new MySimpleEventData(3, tenantId));

    //    Assert.Equal(tenantId, MySimpleDistributedSingleInstanceEventHandler.TenantId);
    //}

    //[Fact]
    //public async Task Should_Change_TenantId_If_Generic_EventData_Is_MultiTenant()
    //{
    //    var tenantId = Guid.NewGuid();

    //    DistributedEventBus.Subscribe<EntityCreatedEto<MySimpleEventData>>(GetRequiredService<MySimpleDistributedSingleInstanceEventHandler>());

    //    await DistributedEventBus.PublishAsync(new MySimpleEventData(3, tenantId));

    //    Assert.Equal(tenantId, MySimpleDistributedSingleInstanceEventHandler.TenantId);
    //}

    //[Fact]
    //public async Task Should_Get_TenantId_From_EventEto_Extra_Property()
    //{
    //    var tenantId = Guid.NewGuid();
        
    //    DistributedEventBus.Subscribe<MySimpleEto>(GetRequiredService<MySimpleDistributedSingleInstanceEventHandler>());

    //    await DistributedEventBus.PublishAsync(new MySimpleEto
    //    {
    //        Properties =
    //        {
    //            {"TenantId", tenantId.ToString()}
    //        }
    //    });
        
    //    Assert.Equal(tenantId, MySimpleDistributedSingleInstanceEventHandler.TenantId);
    //}
}