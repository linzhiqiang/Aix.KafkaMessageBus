﻿using Aix.KafkaMessageBus.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.KafkaMessageBus
{
    public interface IKafkaMessageBus:IDisposable
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task PublishAsync(Type messageType, object message);

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="subscribeOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SubscribeAsync<T>(Func<T, Task> handler, SubscribeOptions  subscribeOptions=null, CancellationToken cancellationToken = default(CancellationToken)) where T : class;

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="subscribeOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SubscribeAsync<T>(Func<T, SubscribeContext, Task> handler, SubscribeOptions subscribeOptions = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class;

        
    }
}
