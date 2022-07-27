using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Plus;
using Volo.Abp.Data;
using Volo.Abp.EventBus;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class DbContextEventEnhancedOutbox<TDbContext> : DbContextEventOutbox<TDbContext>, IDbContextEventOutbox<TDbContext>
    where TDbContext : IHasEventOutbox
{
    public DbContextEventEnhancedOutbox(IDbContextProvider<TDbContext> dbContextProvider, IDistributedEventBus distributedEventBus) : base(dbContextProvider)
    {
        DistributedEventBus = distributedEventBus;
    }

    public IDistributedEventBus DistributedEventBus { get; }

    public override async Task EnqueueAsync(OutgoingEventInfo outgoingEvent)
    {
        var dbContext = (IHasEventOutbox)await DbContextProvider.GetDbContextAsync();
        var outgoingEventRecord = await GetWaitingEventRecordAsync(outgoingEvent.Id);

        if (outgoingEventRecord != null)
        {
            foreach (var prop in outgoingEvent.ExtraProperties)
            {
                outgoingEventRecord.SetProperty(prop.Key, prop.Value);
            }
            dbContext.OutgoingEvents.Update(outgoingEventRecord);
        }
        else
        {
            outgoingEventRecord = new OutgoingEventRecord(outgoingEvent);
            foreach (var prop in outgoingEvent.ExtraProperties)
            {
                outgoingEventRecord.SetProperty(prop.Key, prop.Value);
            }
            dbContext.OutgoingEvents.Add(outgoingEventRecord);
        }

        // Sync outgoing event sending log.
        if (outgoingEvent.EventName != EventNameAttribute.GetNameOrDefault(typeof(OutgoingEventInfoChangedEvent)))
        {
            await DistributedEventBus.PublishAsync(new OutgoingEventInfoChangedEvent(outgoingEvent));
        }
    }

    /// <summary>
    /// Get no handled and re-handle needed event
    /// </summary>
    /// <param name="maxCount"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<List<OutgoingEventInfo>> GetWaitingEventsAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var dbContext = (IHasEventOutbox)await DbContextProvider.GetDbContextAsync();

        var outgoingEventRecords = await dbContext
            .OutgoingEvents
            .AsNoTracking()
            .Where(x => 
                // at least once
                (string)x.ExtraProperties[EventInfoExtraPropertiesConst.Status] != EventInfoStatusConst.Failed
                || ((string)x.ExtraProperties[EventInfoExtraPropertiesConst.Status] == EventInfoStatusConst.Failed 
                    && (DateTime)x.ExtraProperties[EventInfoExtraPropertiesConst.NextRetryTime] <= DateTime.Now)
                )
            .OrderBy(x => x.CreationTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken: cancellationToken);

        return outgoingEventRecords.Select(x =>
        {
            var info = x.ToOutgoingEventInfo();
            foreach (var prop in x.ExtraProperties)
            {
                info.SetProperty(prop.Key, prop.Value);
            }
            return info;
        }).ToList();
    }

    private async Task<OutgoingEventRecord> GetWaitingEventRecordAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var dbContext = (IHasEventOutbox)await DbContextProvider.GetDbContextAsync();

        return await dbContext.OutgoingEvents.FindAsync(eventId);
    }
}
