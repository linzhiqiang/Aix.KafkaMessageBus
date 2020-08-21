using Aix.KafkaMessageBus.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace KafkaMessageBusExample
{
    [TopicAttribute(Name = "demo")]
    [DisplayAttribute(Name = "demo")]
    public class BusinessMessage
    {

        public string RouteKey { get; set; }

        // [RouteKeyAttribute]
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
