using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.KafkaMessageBus
{
    public static class MessageBusExtensions
    {
        public static Task PublishAsync<T>(this IKafkaMessageBus messageBus, T message)
        {
            return messageBus.PublishAsync(typeof(T), message);
        }
      
    }
}
