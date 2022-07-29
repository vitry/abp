using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Plus;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class DbContextEventEnhancedOutbox<TDbContext> : DbContextEventOutbox<TDbContext>, IDbContextEventOutbox<TDbContext>
    where TDbContext : IHasEventOutbox
{
    public DbContextEventEnhancedOutbox(IDbContextProvider<TDbContext> dbContextProvider, IClock clock) : base(dbContextProvider)
    {
        Clock = clock;
    }

    protected IClock Clock { get; }

    [UnitOfWork]
    public override async Task EnqueueAsync(OutgoingEventInfo outgoingEvent)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();
        var record = await dbContext.OutgoingEvents.FindAsync(outgoingEvent.Id);
        if (record != null)
        {
            record.UpdateExtraProperties(outgoingEvent.ExtraProperties);
            dbContext.OutgoingEvents.Update(record);
        }
        else
        {
            record.UpdateExtraProperties(outgoingEvent.ExtraProperties);
            dbContext.OutgoingEvents.Add(record);
        }
    }

    /// <summary>
    /// Get no handled and re-handle needed event
    /// </summary>
    /// <param name="maxCount"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [UnitOfWork]
    public override async Task<List<OutgoingEventInfo>> GetWaitingEventsAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var dbContext = (IHasEventOutbox)await DbContextProvider.GetDbContextAsync();

        var outgoingEventRecords = await dbContext
            .OutgoingEvents
            .AsNoTracking()
            .Where(x =>
                // at least once
                !(bool)x.ExtraProperties[EventInfoExtraPropertiesConst.Failed]
                || ((bool)x.ExtraProperties[EventInfoExtraPropertiesConst.Failed]
                    && (DateTime)x.ExtraProperties[EventInfoExtraPropertiesConst.NextRetryTime] <= Clock.Now)
                )
            .OrderBy(x => x.CreationTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken: cancellationToken);

        return outgoingEventRecords.Select(x => x.ToExtraPropsOutgoingEventInfo()).ToList();
    }
}