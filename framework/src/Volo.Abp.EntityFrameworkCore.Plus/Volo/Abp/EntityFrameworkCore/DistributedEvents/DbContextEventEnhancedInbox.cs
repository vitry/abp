using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Boxes;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Plus;
using Volo.Abp.Timing;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class DbContextEventEnhancedInbox<TDbContext> : DbContextEventInbox<TDbContext>, IDbContextEventInbox<TDbContext>
    where TDbContext : IHasEventInbox
{
    public DbContextEventEnhancedInbox(IDbContextProvider<TDbContext> dbContextProvider, IClock clock, IOptions<AbpEventBusBoxesOptions> eventBusBoxesOptions, IDistributedEventBus distributedEventBus) : base(dbContextProvider, clock, eventBusBoxesOptions)
    {
        DistributedEventBus = distributedEventBus;
    }

    protected IDistributedEventBus DistributedEventBus { get; }

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
        await PublishIncomingEventInfoChangedEvent(incomingEvent);
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
                ((string)x.ExtraProperties[EventInfoExtraPropertiesConst.Status] != EventInfoStatusConst.Failed
                || ((string)x.ExtraProperties[EventInfoExtraPropertiesConst.Status] == EventInfoStatusConst.Failed
                    && (DateTime)x.ExtraProperties[EventInfoExtraPropertiesConst.NextRetryTime] <= DateTime.Now))
            )
            .OrderBy(x => x.CreationTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken: cancellationToken);

        return incomingEventRecords.Select(x => x.ToExtraPropsIncomingEventInfo()).ToList();
    }

    public override async Task MarkAsProcessedAsync(Guid id)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();
        var incomingEvent = await dbContext.IncomingEvents.FindAsync(id);
        if (incomingEvent != null)
        {
            incomingEvent.MarkAsProcessed(Clock.Now);
            await PublishIncomingEventInfoChangedEvent(incomingEvent.ToExtraPropsIncomingEventInfo());
        }
    }

    private async Task PublishIncomingEventInfoChangedEvent(IncomingEventInfo incomingEvent)
    {
        if (incomingEvent.EventName != EventNameAttribute.GetNameOrDefault(typeof(IncomingEventInfoChangedEvent)))
        {
            await DistributedEventBus.PublishAsync(new IncomingEventInfoChangedEvent(incomingEvent));
        }
    }
}