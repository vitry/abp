using System;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EventBus.Plus;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;

namespace Volo.Abp.EntityFrameworkCore;

public static class EnhancedBoxEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            ObjectExtensionManager.Instance.MapEfCoreProperty<OutgoingEventRecord, bool>(EventInfoExtraPropertiesConst.Failed);
            ObjectExtensionManager.Instance.MapEfCoreProperty<OutgoingEventRecord, DateTime>(EventInfoExtraPropertiesConst.NextRetryTime);
            ObjectExtensionManager.Instance.MapEfCoreProperty<IncomingEventRecord, bool>(EventInfoExtraPropertiesConst.Failed);
            ObjectExtensionManager.Instance.MapEfCoreProperty<IncomingEventRecord, DateTime>(EventInfoExtraPropertiesConst.NextRetryTime);
        });
    }
}