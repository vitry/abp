using System;
using System.Text;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Enq = EasyNetQ;

namespace Volo.Abp.EventBus.EasyNetQ;

public class Utf8JsonEasyNetQSerializer : IEasyNetQSerializer, ITransientDependency
{
    private readonly IJsonSerializer _jsonSerializer;

    public Utf8JsonEasyNetQSerializer(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public object Deserialize(byte[] value, Type type)
    {
        return _jsonSerializer.Deserialize(type, Encoding.UTF8.GetString(value));   
    }

    public T Deserialize<T>(byte[] value)
    {
        return _jsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(value));
    }

    public byte[] Serialize(object obj)
    {
        return Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(obj));
    }
}