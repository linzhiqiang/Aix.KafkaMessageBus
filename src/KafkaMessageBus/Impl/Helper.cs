using Aix.KafkaMessageBus.Model;
using Aix.KafkaMessageBus.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Aix.KafkaMessageBus.Impl
{
    internal static class Helper
    {
        public static string GetTopic(KafkaMessageBusOptions options, Type type)
        {
            string topicName = type.Name;

            var topicAttr = TopicAttribute.GetTopicAttribute(type);
            if (topicAttr != null && !string.IsNullOrEmpty(topicAttr.Name))
            {
                topicName = topicAttr.Name;
            }
            else
            {
                var displayAttr = AttributeUtils.GetAttribute<DisplayAttribute>(type);
                if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
                {
                    topicName = displayAttr.Name;
                }
            }

            return $"{options.TopicPrefix ?? ""}{topicName}";
        }

        public static string GetKey(object message)
        {
            var keyValue = AttributeUtils.GetPropertyValue<RouteKeyAttribute>(message);
            //if (keyValue == null)
            //{
            //    keyValue = AttributeUtils.GetPropertyValue<KeyAttribute>(message);
            //}
            return keyValue != null ? keyValue.ToString() : null;
        }
    }


}
