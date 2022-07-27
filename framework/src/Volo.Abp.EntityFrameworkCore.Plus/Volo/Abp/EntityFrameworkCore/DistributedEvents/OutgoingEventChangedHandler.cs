using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Plus;

namespace Volo.Abp.EntityFrameworkCore.DistributedEvents;

public class OutgoingEventChangedHandler : IDistributedEventHandler<OutgoingEventInfoChangedEvent>, ITransientDependency
{
    private readonly ILogger<OutgoingEventChangedHandler> _logger;

    public OutgoingEventChangedHandler(ILogger<OutgoingEventChangedHandler> logger)
    {
        this._logger = logger;
    }

    public Task HandleEventAsync(OutgoingEventInfoChangedEvent @event)
    {
        string logContent = $"{@event.EventName} {@event.EventId} publish log sended"; 
        Console.WriteLine(logContent);
        _logger.LogDebug(logContent);
        return Task.CompletedTask;
    }
}