using System;
using System.Linq;
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
        int messageCount = 10;
        foreach (var data in Enumerable.Range(1, messageCount))
        {
            await DistributedEventBus.PublishAsync(new MySimpleEventData(data));
        }
        // wait for handle
        await Task.Delay(1000*10);

        int expected = messageCount;

        // error when run tests with Should_Change_TenantId_If_EventData_Is_MultiTenant() 
        Assert.Equal(expected, MySimpleDistributedTransientEventHandler.HandleCount);
        Assert.Equal(expected, MySimpleDistributedTransientEventHandler.DisposeCount);
    }

    [Fact]
    public async Task Should_Change_TenantId_If_EventData_Is_MultiTenant()
    {
        var tenantId = Guid.NewGuid();
        await DistributedEventBus.PublishAsync(new MySimpleEventData(3, tenantId));
        // wait for handle
        await Task.Delay(1000*10);

        Assert.Equal(tenantId, MySimpleDistributedSingleInstanceEventHandler.TenantId);
    }

    [Fact]
    public async Task Should_Change_TenantId_If_Generic_EventData_Is_MultiTenant()
    {
        var tenantId = Guid.NewGuid();
        await DistributedEventBus.PublishAsync(new EntityCreatedEto<MySimpleEventData>(new MySimpleEventData(3, tenantId)));
        // wait for handle
        await Task.Delay(1000*10);
        Assert.Equal(tenantId, MySimpleDistributedSingleInstanceEventHandler.TenantId);
    }

    [Fact]
    public async Task Should_Get_TenantId_From_EventEto_Extra_Property()
    {
        var tenantId = Guid.NewGuid();
        await DistributedEventBus.PublishAsync(new MySimpleEto
        {
            Properties =
            {
                {"TenantId", tenantId.ToString()}
            }
        });
        // wait for handle
        await Task.Delay(1000*10);
        Assert.Equal(tenantId, MySimpleDistributedSingleInstanceEventHandler.TenantId);
    }
}