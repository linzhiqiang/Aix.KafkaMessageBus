using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.KafkaMessageBus.Model
{
    public class KafkaMessageBusData
    {
        public string Topic { get; set; }
        public byte[] Data { get; set; }
    }
}
