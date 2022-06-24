using System;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.EasyNetQ;

public class MySimpleDistributedTransientEventHandler : IDistributedEventHandler<MySimpleEventData>, IDisposable
{
    public static int HandleCount { get; set; }

    public static int DisposeCount { get; set; }

    public Task HandleEventAsync(MySimpleEventData eventData)
    {
        ++HandleCount;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        ++DisposeCount;
    }
}
