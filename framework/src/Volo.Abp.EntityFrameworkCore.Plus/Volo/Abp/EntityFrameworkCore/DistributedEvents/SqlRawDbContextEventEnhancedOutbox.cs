using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class SqlRawDbContextEventEnhancedOutbox<TDbContext> : DbContextEventEnhancedOutbox<TDbContext>, ISqlRawDbContextEventOutbox<TDbContext>
    where TDbContext : IHasEventOutbox
{
    public SqlRawDbContextEventEnhancedOutbox(IDbContextProvider<TDbContext> dbContextProvider, IDistributedEventBus distributedEventBus) : base(dbContextProvider, distributedEventBus)
    {
    }
}