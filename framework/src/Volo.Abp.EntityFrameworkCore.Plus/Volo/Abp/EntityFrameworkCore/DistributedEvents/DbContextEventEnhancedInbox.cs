using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Plus;
using Volo.Abp.Timing;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class DbContextEventEnhancedInbox<TDbContext> : DbContextEventInbox<TDbContext>, IDbContextEventInbox<TDbContext>
    where TDbContext : IHasEventInbox
{
    public DbContextEventEnhancedInbox(
        IDbContextProvider<TDbContext> dbContextProvider,
        IClock clock,
        IOptions<AbpEventBusBoxesOptions> eventBusBoxesOptions)
        : base(dbContextProvider, clock, eventBusBoxesOptions)
    {
    }

    public override async Task EnqueueAsync(IncomingEventInfo incomingEvent)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();
        var record = await dbContext.IncomingEvents.FindAsync(incomingEvent.Id);
        if (record != null)
        {
            record.UpdateExtraProperties(incomingEvent.ExtraProperties);
            dbContext.IncomingEvents.Update(record);
        }
        else
        {
            record = new IncomingEventRecord(incomingEvent);
            record.UpdateExtraProperties(incomingEvent.ExtraProperties);
            dbContext.IncomingEvents.Add(record);
        }
    }

    public override async Task<List<IncomingEventInfo>> GetWaitingEventsAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();

        var incomingEventRecords = await dbContext
            .IncomingEvents
            .AsNoTracking()
            .Where(x =>
                // at least once
                !x.Processed &&
                (!(bool)x.ExtraProperties[EventInfoExtraPropertiesConst.Failed]
                || ((bool)x.ExtraProperties[EventInfoExtraPropertiesConst.Failed]
                    && (DateTime)x.ExtraProperties[EventInfoExtraPropertiesConst.NextRetryTime] <= Clock.Now))
            )
            .OrderBy(x => x.CreationTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken: cancellationToken);

        return incomingEventRecords.Select(x => x.ToExtraPropsIncomingEventInfo()).ToList();
    }
}