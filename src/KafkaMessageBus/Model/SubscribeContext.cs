using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.KafkaMessageBus.Model
{
    public class SubscribeContext
    {
        public string Topic { get; set; }

        public string GroupId { get; set; }
    }
}
