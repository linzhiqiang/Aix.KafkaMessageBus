﻿using Aix.KafkaMessageBus.Model;
using System;

namespace KafkaMessageBusExample
{
    [TopicAttribute(Name = "BusinessMessage")]
    public class BusinessMessage
    {
        [RouteKeyAttribute]
        public string MessageId { get; set; }
        public string Content { get; set; }

        public DateTime CreateTime { get; set; }
    }

    [TopicAttribute(Name = "BusinessMessage2")]
    public class BusinessMessage2
    {
        public string MessageId { get; set; }
        public string Content { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
