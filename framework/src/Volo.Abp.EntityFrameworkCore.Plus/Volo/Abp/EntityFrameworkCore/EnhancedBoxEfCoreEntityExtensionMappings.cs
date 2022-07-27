using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EventBus.Plus;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;
using Microsoft.EntityFrameworkCore;
using System;

namespace Volo.Abp.EntityFrameworkCore;

public static class EnhancedBoxEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            ObjectExtensionManager.Instance.MapEfCoreProperty<OutgoingEventRecord, string>(
                EventInfoExtraPropertiesConst.Status,
                (entityBuilder, propertyBuilder) =>
                {
                    propertyBuilder.HasMaxLength(10);
                    propertyBuilder.HasDefaultValue(EventInfoStatusConst.Scheduled);
                }
                );
            ObjectExtensionManager.Instance.MapEfCoreProperty<OutgoingEventRecord, DateTime>(EventInfoExtraPropertiesConst.NextRetryTime);
        });
    }
}