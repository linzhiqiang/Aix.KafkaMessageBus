﻿using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Aix.KafkaMessageBus.Utils;
using System.Linq;
using Aix.KafkaMessageBus.Serializer;

namespace Aix.KafkaMessageBus.Impl
{
    /// <summary>
    /// kafka消费者
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>
    {
        private IServiceProvider _serviceProvider;
        private ILogger<KafkaConsumer<TKey, TValue>> _logger;
        private KafkaMessageBusOptions _kafkaOptions;


        IConsumer<TKey, TValue> _consumer = null;
        /// <summary>
        /// 存储每个分区的最大offset，针对手工提交 
        /// </summary>
        private ConcurrentDictionary<TopicPartition, TopicPartitionOffset> _currentOffsets = new ConcurrentDictionary<TopicPartition, TopicPartitionOffset>();
        private volatile bool _isStart = false;
        private int Count = 0;
        private DateTime LastCommitTime = DateTime.MaxValue;
        private int CancellationDelayMaxMs = 0;

        public event Func<ConsumeResult<TKey, TValue>, Task> OnMessage;
        public KafkaConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _logger = serviceProvider.GetService<ILogger<KafkaConsumer<TKey, TValue>>>();
            _kafkaOptions = serviceProvider.GetService<KafkaMessageBusOptions>();
            CancellationDelayMaxMs = _kafkaOptions.CancellationDelayMaxMs > 0 ? _kafkaOptions.CancellationDelayMaxMs : 100;
        }

        public async Task Subscribe(string topic, string groupId, CancellationToken cancellationToken)
        {
            //return Task.Run(async () =>
            //{
            //    _isStart = true;
            //    this._consumer = this.CreateConsumer(groupId);
            //    this._consumer.Subscribe(topic);
            //    await StartPoll(cancellationToken);
            //});

            _isStart = true;
            this._consumer = this.CreateConsumer(groupId);
            this._consumer.Subscribe(topic);
            await StartPoll(cancellationToken);
        }

        public void Close()
        {
            if (this._isStart == false) return;
            lock (this)
            {
                if (this._isStart == false) return;
                this._isStart = false;
            }
            _logger.LogInformation("Kafka关闭消费者");
            ManualCommitOffset();
            With.NoException(_logger, () => { this._consumer?.Close(); }, "关闭消费者");
        }

        public void Dispose()
        {
            this.Close();
        }

        #region private

        private Task StartPoll(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () =>
            {
                _logger.LogInformation("开始消费数据...");
                LastCommitTime = DateTime.Now;
                try
                {
                    while (_isStart && !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            await Consumer(cancellationToken);//AccessViolationException

                            ManualTimeoutCommitOffset(); //处理间隔多长时间主动提交，针对手工提交配置
                        }
                        catch (OperationCanceledException)
                        {
                            //_logger.LogError(ex, $"消费异常退出消费循环OperationCanceledException，操作取消");
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, $"消费拉取消息ConsumeException");
                        }
                        catch (KafkaException ex)
                        {
                            _logger.LogError(ex, $"消费拉取消息KafkaException");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"消费拉取消息系统异常Exception");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"消费异常退出消费循环Exception");
                }
                finally
                {
                    _logger.LogInformation("退出消费循环 关闭消费者...");
                    this.Close();
                }
            });

            return Task.CompletedTask;
        }

        private async Task Consumer(CancellationToken cancellationToken)
        {
            //var result = this._consumer.Consume(cancellationToken);// cancellationToken默认100毫秒
            var result = this._consumer.Consume(CancellationDelayMaxMs);
            if (result == null || result.IsPartitionEOF || result.Message == null || result.Message.Value == null)
            {
                return;
            }
            //消费数据
            await Handler(result);

            //处理手动提交
            ManualCommitOffset(result); //采用后提交（至少一次）,消费前提交（至多一次）
        }

        private async Task Handler(ConsumeResult<TKey, TValue> consumeResult)
        {
            if (OnMessage == null) return;
            try
            {
                await OnMessage(consumeResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "kafka消费失败");
            }
        }

        private void AddToOffsetDict(TopicPartition topicPartition, TopicPartitionOffset TopicPartitionOffset)
        {
            _currentOffsets.AddOrUpdate(topicPartition, TopicPartitionOffset, (key, oldValue) =>
            {
                return TopicPartitionOffset.Offset > oldValue.Offset ? TopicPartitionOffset : oldValue;
            });
        }


        /// <summary>
        /// 手工提交offset
        /// </summary>
        /// <param name="result"></param>
        private void ManualCommitOffset(ConsumeResult<TKey, TValue> result)
        {
            //处理手动提交
            if (EnableAutoCommit() == false)
            {
                Count++;
                var topicPartition = result.TopicPartition;
                var topicPartitionOffset = new TopicPartitionOffset(topicPartition, result.Offset + 1);
                AddToOffsetDict(topicPartition, topicPartitionOffset); //加入offset缓存 

                if (Count % _kafkaOptions.ManualCommitBatch == 0)
                {
                    ManualCommitOffset();
                }
                //else
                //{
                //    ManualTimeoutCommitOffset();
                //}
            }
        }

        /// <summary>
        /// 超过配置时间提交
        /// </summary>
        private void ManualTimeoutCommitOffset()
        {
            if (_kafkaOptions.ManualCommitIntervalSecond > 0 && EnableAutoCommit() == false)
            {
                var isTimeout = (DateTime.Now - LastCommitTime).TotalSeconds > _kafkaOptions.ManualCommitIntervalSecond;
                if (isTimeout)
                {
                    ManualCommitOffset();
                }
            }
        }

        /// <summary>
        /// 提交所有分区
        /// </summary>
        private void ManualCommitOffset()
        {
            With.NoException(_logger, () =>
            {
                //foreach (var item in _currentOffsets)
                //{
                //    // _logger.LogInformation($"--------------------手动提交偏移量分区：{ item.Key.Partition.Value}----------------");
                //    With.NoException(_logger, () =>
                //    {
                //        this._consumer.Commit(new[] { item.Value });
                //    }, $"手动提交偏移量分区：{item.Key.Partition.Value}");
                //}

                if (_currentOffsets.Count > 0)
                {
                    LastCommitTime = DateTime.Now;
                    //_logger.LogInformation($"--------------------手动提交偏移量分区：{ string.Join(",", _currentOffsets.Values)}----------------");
                    this._consumer.Commit(_currentOffsets.Values);
                }
            }, "手动提交所有分区错误");

            ClearCurrentOffsets();
        }

        private void ClearCurrentOffsets()
        {
            Count = 0;
            _currentOffsets.Clear();
        }


        /// <summary>
        /// 创建消费者对象
        /// </summary>
        /// <returns></returns>
        private IConsumer<TKey, TValue> CreateConsumer(string groupId)
        {
            if (_kafkaOptions.ConsumerConfig == null) _kafkaOptions.ConsumerConfig = new ConsumerConfig();

            if (string.IsNullOrEmpty(_kafkaOptions.ConsumerConfig.BootstrapServers))
            {
                _kafkaOptions.ConsumerConfig.BootstrapServers = _kafkaOptions.BootstrapServers;

            }
            if (string.IsNullOrEmpty(_kafkaOptions.ConsumerConfig.BootstrapServers))
            {
                throw new Exception("请配置BootstrapServers参数");
            }

            var config = new Dictionary<string, string>(); //这里转成字典 便于不同消费者可以改变消费者配置（因为是就一个配置对象）
            lock (_kafkaOptions.ConsumerConfig)
            {
                config = _kafkaOptions.ConsumerConfig.ToDictionary(x => x.Key, v => v.Value);
            }
            if (!string.IsNullOrEmpty(groupId))
            {
                config["group.id"] = groupId;
            }

            var builder = new ConsumerBuilder<TKey, TValue>(config)
                 .SetErrorHandler((producer, error) =>
                 {
                     if (error.IsFatal || error.IsBrokerError)
                     {
                         string errorInfo = $"Code:{error.Code}, Reason:{error.Reason}, IsFatal={error.IsFatal}, IsLocalError:{error.IsLocalError}, IsBrokerError:{error.IsBrokerError}";
                         _logger.LogError($"Kafka消费者出错：{errorInfo}");
                     }
                 })
                 .SetPartitionsRevokedHandler((c, partitions) =>
                 {
                     //方法会在再均衡开始之前和消费者停止读取消息之后被调用。如果在这里提交偏移量，下一个接管partition的消费者就知道该从哪里开始读取了。
                     //partitions表示再均衡前所分配的分区
                     if (EnableAutoCommit() == false)
                     {
                         ManualCommitOffset();
                     }
                 })
                 .SetPartitionsAssignedHandler((c, partitions) =>
                 {
                     //方法会在重新分配partition之后和消费者开始读取消息之前被调用。
                     if (EnableAutoCommit() == false)
                     {
                         ClearCurrentOffsets();
                     }
                     //_logger.LogInformation($"MemberId:{c.MemberId}分配的分区：Assigned partitions: [{string.Join(", ", partitions)}]");
                     _logger.LogInformation($"MemberId:{c.MemberId}分配的分区：{DisplayTopicPartitions(partitions)}");
                 })
               .SetValueDeserializer(new ConfluentKafkaSerializerAdapter<TValue>(_kafkaOptions.Serializer));

            //以下是内置的
            //if (typeof(TKey) == typeof(Null)) builder.SetKeyDeserializer((IDeserializer<TKey>)Confluent.Kafka.Deserializers.Null);
            //if (typeof(TKey) == typeof(string)) builder.SetKeyDeserializer((IDeserializer<TKey>)Confluent.Kafka.Deserializers.Utf8);
            //if (typeof(TKey) == typeof(int)) builder.SetKeyDeserializer((IDeserializer<TKey>)Confluent.Kafka.Deserializers.Int32);
            //if (typeof(TKey) == typeof(long)) builder.SetKeyDeserializer((IDeserializer<TKey>)Confluent.Kafka.Deserializers.Int64);
            //if (typeof(TKey) == typeof(float)) builder.SetKeyDeserializer((IDeserializer<TKey>)Confluent.Kafka.Deserializers.Single);
            //if (typeof(TKey) == typeof(double)) builder.SetKeyDeserializer((IDeserializer<TKey>)Confluent.Kafka.Deserializers.Double);
            //if (typeof(TKey) == typeof(byte[])) builder.SetKeyDeserializer((IDeserializer<TKey>)Confluent.Kafka.Deserializers.ByteArray);
            //if (typeof(TKey) == typeof(Ignore)) builder.SetKeyDeserializer((IDeserializer<TKey>)Confluent.Kafka.Deserializers.Ignore);
            //if (typeof(TKey) == typeof(object)) builder.SetKeyDeserializer(new ConfluentKafkaSerializerAdapter<TKey>(_kafkaOptions.Serializer));

            var consumer = builder.Build();
            return consumer;
        }

        /// <summary>
        /// 是否是自动提交
        /// </summary>
        /// <returns></returns>
        private bool EnableAutoCommit()
        {
            var enableAutoCommit = this._kafkaOptions.ConsumerConfig.EnableAutoCommit;
            return !enableAutoCommit.HasValue || enableAutoCommit.Value == true;
        }

        private string DisplayTopicPartitions(List<TopicPartition> topicPartitions)
        {
            StringBuilder sb = new StringBuilder();
            if (topicPartitions != null)
            {
                foreach (var group in topicPartitions.GroupBy(x => x.Topic))
                {
                    sb.Append($"{group.Key}[{string.Join(",", group.Select(x => x.Partition.Value))}]");
                }

            }
            return sb.ToString();
        }

        #endregion
    }
}
