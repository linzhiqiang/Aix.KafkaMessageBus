using Aix.KafkaMessageBus.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace KafkaMessageBusExample
{
    //[TopicAttribute(Name = "BusinessMessage")]
    [DisplayAttribute(Name = "ServiceCallInfo")]
    public class BusinessMessage
    {
      //  [RouteKeyAttribute]
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
