using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.Modularity;

namespace Volo.Abp.EntityFrameworkCore;

[DependsOn(typeof(AbpEntityFrameworkCoreModule))]
public class AbpEntityFrameworkCorePlusModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        EnhancedBoxEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Transient(typeof(IDbContextEventOutbox<>), typeof(DbContextEventEnhancedOutbox<>)));
        context.Services.Replace(ServiceDescriptor.Transient(typeof(IDbContextEventInbox<>), typeof(DbContextEventEnhancedInbox<>)));

        // must exclude event 'OutgoingEventInfoChangedEvent'
    }
}