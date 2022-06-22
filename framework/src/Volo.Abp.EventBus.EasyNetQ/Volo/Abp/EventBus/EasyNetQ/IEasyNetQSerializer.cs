using System;

namespace Volo.Abp.EventBus.EasyNetQ.Volo.Abp.EventBus.EasyNetQ;

public interface IEasyNetQSerializer
{
    byte[] Serialize(object obj);

    object Deserialize(byte[] value, Type type);

    T Deserialize<T>(byte[] value);
}