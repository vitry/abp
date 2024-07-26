using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EasyNetQ;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.EasyNetQ;

public class BusPool : IBusPool, ISingletonDependency
{
    private readonly AbpEasyNetQOptions _options;
    private bool _isDisposed;

    protected ConcurrentDictionary<string, Lazy<IBus>> LazyBuses { get; }

    public BusPool(IOptions<AbpEasyNetQOptions> options)
    {
        _options = options.Value;
        LazyBuses = new ConcurrentDictionary<string, Lazy<IBus>>();
    }

    public virtual IBus Get(string? busName = null)
    {
        busName ??= EasyNetQBuses.DefaultBusName;

        try
        {
            var lazyBus = LazyBuses.GetOrAdd(
                busName, () => new Lazy<IBus>(() =>
                {
                    string connection = _options.Buses.GetOrDefault(busName);
                    return RabbitHutch.CreateBus(connection, _options.BusServiceRegister);
                })
                );
            return lazyBus.Value;
        }
        catch (Exception)
        {
            LazyBuses.TryRemove(busName, out _);
            throw;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        foreach (var lazyBus in LazyBuses.Values)
        {
            try
            {
                lazyBus.Value.Dispose();
            }
            catch (Exception)
            {
                throw;
            }
        }
        LazyBuses.Clear();
    }
}