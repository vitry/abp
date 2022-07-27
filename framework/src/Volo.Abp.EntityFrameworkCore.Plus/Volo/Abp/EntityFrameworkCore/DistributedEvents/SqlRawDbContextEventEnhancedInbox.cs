namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class SqlRawDbContextEventEnhancedInbox<TDbContext> : DbContextEventEnhancedInbox<TDbContext>, ISqlRawDbContextEventInbox<TDbContext>
    where TDbContext : IHasEventInbox
{
}