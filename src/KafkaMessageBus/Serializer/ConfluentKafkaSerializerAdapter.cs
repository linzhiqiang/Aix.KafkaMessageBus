using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.KafkaMessageBus.Serializer
{
    /// <summary>
    /// kafka序列化适配  转化为Confluent.Kafka的api要求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ConfluentKafkaSerializerAdapter<T> : Confluent.Kafka.ISerializer<T>, Confluent.Kafka.IDeserializer<T>
    {
        private ISerializer _serializer;
        public ConfluentKafkaSerializerAdapter(ISerializer serializer)
        {
            _serializer = serializer;
        }
        public byte[] Serialize(T data, SerializationContext context)
        {
            return _serializer.Serialize<T>(data);
        }

        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (!isNull)
            {
                return _serializer.Deserialize<T>(data.ToArray());
            }
            return default(T);
        }

    }

    internal class ConfluentKafkaKeySerializerAdapter<T> : Confluent.Kafka.ISerializer<T>, Confluent.Kafka.IDeserializer<T>
    {
        private ISerializer _serializer;
        public ConfluentKafkaKeySerializerAdapter(ISerializer serializer)
        {
            _serializer = serializer;
        }
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(data?.ToString());
        }

        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (!isNull)
            {
                var key = Encoding.UTF8.GetString(data.ToArray());
                return (T)Convert.ChangeType(key,typeof(T));
            }
            return default(T);
        }

    }
}
