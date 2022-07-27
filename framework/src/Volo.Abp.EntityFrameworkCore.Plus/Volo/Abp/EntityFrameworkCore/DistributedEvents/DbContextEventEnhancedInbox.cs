using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class DbContextEventEnhancedInbox<TDbContext> : IDbContextEventInbox<TDbContext>
    where TDbContext : IHasEventInbox
{
    public Task DeleteOldEventsAsync()
    {
        throw new NotImplementedException();
    }

    public Task EnqueueAsync(IncomingEventInfo incomingEvent)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsByMessageIdAsync(string messageId)
    {
        throw new NotImplementedException();
    }

    public Task<List<IncomingEventInfo>> GetWaitingEventsAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task MarkAsProcessedAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}