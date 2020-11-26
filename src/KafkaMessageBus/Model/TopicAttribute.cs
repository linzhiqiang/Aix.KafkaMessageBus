using Aix.KafkaMessageBus.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.KafkaMessageBus.Model
{
    public class TopicAttribute : Attribute
    {
        /// <summary>
        /// Topic
        /// </summary>
        public string Name { get; set; }

        public TopicAttribute()
        {
        }

        public static TopicAttribute GetTopicAttribute(Type type)
        {
            return AttributeUtils.GetAttribute<TopicAttribute>(type);

        }
    }

}
