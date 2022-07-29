using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class SqlRawDbContextEventEnhancedOutbox<TDbContext> : DbContextEventEnhancedOutbox<TDbContext>, ISqlRawDbContextEventOutbox<TDbContext>
    where TDbContext : IHasEventOutbox
{
    public SqlRawDbContextEventEnhancedOutbox(IDbContextProvider<TDbContext> dbContextProvider, IClock clock)
        : base(dbContextProvider, clock)
    {
    }

    [UnitOfWork]
    public override async Task DeleteAsync(Guid id)
    {
        var dbContext = (IHasEventOutbox)await DbContextProvider.GetDbContextAsync();
        var tableName = dbContext.OutgoingEvents.EntityType.GetSchemaQualifiedTableName();

        var sql = $"DELETE FROM {tableName} WHERE Id = '{id.ToString().ToUpper()}'";
        await dbContext.Database.ExecuteSqlRawAsync(sql);
    }
}