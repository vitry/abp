using System;

namespace Volo.Abp.EasyNetQ;

public interface IEasyNetQSerializer
{
    byte[] Serialize(object obj);

    object Deserialize(byte[] value, Type type);

    T Deserialize<T>(byte[] value);
}