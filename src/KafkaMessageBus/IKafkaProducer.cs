using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.KafkaMessageBus
{
    internal interface IKafkaProducer<TKey, TValue> : IDisposable
    {
        Task<DeliveryResult<TKey, TValue>> ProduceAsync(string topic, Message<TKey, TValue> message);
    }
}
