using System;
using EasyNetQ;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.EasyNetQ;

public interface IBusPool : IDisposable, ISingletonDependency
{
    IBus Get(string? busName = null);
}