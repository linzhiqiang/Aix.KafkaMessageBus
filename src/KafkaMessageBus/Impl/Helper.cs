using Aix.KafkaMessageBus.Model;
using Aix.KafkaMessageBus.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace Aix.KafkaMessageBus.Impl
{
    internal static class Helper
    {
        /// <summary>
        /// topic缓存
        /// </summary>
        private static ConcurrentDictionary<Type, string> TopicCache = new ConcurrentDictionary<Type, string>();

        private static ConcurrentDictionary<Type, PropertyInfo> RouteKeyCache = new ConcurrentDictionary<Type, PropertyInfo>();
        public static string GetTopic(KafkaMessageBusOptions options, Type type)
        {
            string topicName = null;

            if (TopicCache.TryGetValue(type, out topicName))
            {
                return topicName;
            }

            if (!string.IsNullOrEmpty(options.Topic))
            {
                topicName = options.Topic;
            }
            else
            {

                topicName = type.Name; //默认等于该类型的名称
                var topicAttr = TopicAttribute.GetTopicAttribute(type);
                if (topicAttr != null && !string.IsNullOrEmpty(topicAttr.Name))
                {
                    topicName = topicAttr.Name;
                }
                else
                {
                    //var displayAttr = AttributeUtils.GetAttribute<DisplayAttribute>(type);
                    //if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
                    //{
                    //    topicName = displayAttr.Name;
                    //}
                }
                topicName = $"{options.TopicPrefix ?? ""}{topicName}";
            }


            TopicCache.TryAdd(type, topicName);

            return topicName;
        }

        public static string GetKey(object message)
        {
            if (message == null) return null;

            var type = message.GetType();
            PropertyInfo property = null;
            if (RouteKeyCache.TryGetValue(type, out property))
            {

            }
            else
            {
                property = AttributeUtils.GetProperty<RouteKeyAttribute>(message);
                //if (property == null) property = AttributeUtils.GetProperty<KeyAttribute>(message);
                RouteKeyCache.TryAdd(type, property);
            }
            
            var keyValue = property?.GetValue(message);
            return keyValue != null ? keyValue.ToString() : null;
        }
    }


}
