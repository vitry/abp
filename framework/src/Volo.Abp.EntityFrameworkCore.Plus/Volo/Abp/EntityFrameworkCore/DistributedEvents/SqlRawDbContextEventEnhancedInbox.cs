using Microsoft.Extensions.Options;
using Volo.Abp.EventBus.Boxes;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class SqlRawDbContextEventEnhancedInbox<TDbContext> : DbContextEventEnhancedInbox<TDbContext>, ISqlRawDbContextEventInbox<TDbContext>
    where TDbContext : IHasEventInbox
{
    public SqlRawDbContextEventEnhancedInbox(IDbContextProvider<TDbContext> dbContextProvider, IClock clock, IOptions<AbpEventBusBoxesOptions> eventBusBoxesOptions, IDistributedEventBus distributedEventBus) : base(dbContextProvider, clock, eventBusBoxesOptions, distributedEventBus)
    {
    }
}