﻿using Aix.KafkaMessageBus.Serializer;
using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.KafkaMessageBus
{
    /// <summary>
    /// kafka配置
    /// </summary>
    public class KafkaMessageBusOptions
    {
        public KafkaMessageBusOptions()
        {
            this.TopicPrefix = "";//demo-messagebus-
            this.Serializer = SerializerFactory.Instance.GetSerialize();
            this.DefaultConsumerThreadCount = 2;
            this.ManualCommitBatch = 100;
            this.ManualCommitIntervalSecond = 0;
            this.CancellationDelayMaxMs = 100;

        }

        /// <summary>
        /// 生产者配置
        /// </summary>
        public ProducerConfig ProducerConfig { get; set; }

        /// <summary>
        /// 消费者配置
        /// </summary>
        public ConsumerConfig ConsumerConfig { get; set; }

        /// <summary>
        /// kafka集群地址 多个时逗号分隔
        /// </summary>
        public string BootstrapServers { get; set; }

        /// <summary>
        /// topic前缀，为了防止重复，建议用项目名称
        /// </summary>
        public string TopicPrefix { get; set; }

        /// <summary>
        /// 自定义序列化，默认为MessagePack
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// 默认每个Topic的消费线程数 默认2个,请注意与分区数的关系
        /// </summary>
        public int DefaultConsumerThreadCount { get; set; }

        /// <summary>
        /// EnableAutoCommit=false时每多少个消息提交一次 默认100条消息提交一次，重要业务建议手工提交
        /// </summary>
        public int ManualCommitBatch { get; set; }

        /// <summary>
        /// EnableAutoCommit=false时 每多少秒提交一次 默认0秒不开启  ManualCommitBatch和ManualCommitIntervalSecond是或的关系
        /// </summary>
        public int ManualCommitIntervalSecond { get; set; }

        /// <summary>
        /// 循环拉去间隔时间 默认100(毫秒)
        /// </summary>
        public int CancellationDelayMaxMs { get; set; }

        /// <summary>
        /// topic 若果配置了该选项，所有发布订阅都使用该主题 ,兼容老版本问题，请勿配置
        /// </summary>
        public string Topic { get; set; }

    }
}
