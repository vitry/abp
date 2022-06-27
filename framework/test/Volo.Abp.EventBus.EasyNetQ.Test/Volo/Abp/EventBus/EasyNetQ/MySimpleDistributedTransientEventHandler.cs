using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.EasyNetQ;

public class MySimpleDistributedTransientEventHandler : IDistributedEventHandler<MySimpleEventData>, IDisposable
{
    private static int _handleCount = 0;
    public static int HandleCount => _handleCount;

    private static int _disposeCount = 0;
    public static int DisposeCount => _disposeCount;

    public async Task HandleEventAsync(MySimpleEventData eventData)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        Interlocked.Increment(ref _handleCount);
    }

    public void Dispose()
    {
        Interlocked.Increment(ref _disposeCount);
    }
}
