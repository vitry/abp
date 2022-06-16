using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.EasyNetQ.Volo.Abp.EasyNetQ;

public class Connection : IConnection, ISingletonDependency
{
    private readonly AbpEasyNetQOptions _options;
    private IBus _bus;
    private ILogger<Connection> _logger;

    public Connection(IOptions<AbpEasyNetQOptions> options)
    {
        this._options = options.Value;
        this._bus = RabbitHutch.CreateBus(_options.Connection);
    }

    public IBus Bus => _bus;

    public void Dispose()
    {
        _bus.Dispose();
    }
}