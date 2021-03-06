﻿using Aix.KafkaMessageBus;
using KafkaMessageBusExample.HostedService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Text;

namespace KafkaMessageBusExample
{
    public class Startup
    {
        internal static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var options = CmdOptions.Options;
            services.AddSingleton(options);
            var kafkaMessageBusOptions = context.Configuration.GetSection("kafka").Get<KafkaMessageBusOptions>();
            var cancellationDelayMaxMs = context.Configuration.GetSection("kafka:ConsumerConfig:CancellationDelayMaxMs").Get<int?>();
            if (cancellationDelayMaxMs.HasValue) kafkaMessageBusOptions.ConsumerConfig.CancellationDelayMaxMs = cancellationDelayMaxMs.Value;
            services.AddKafkaMessageBus(kafkaMessageBusOptions);

            if ((options.Mode & (int)ClientMode.Consumer) > 0)
            {
                services.AddHostedService<MessageBusConsumeService>();
            }
            if ((options.Mode & (int)ClientMode.Producer) > 0)
            {
                services.AddHostedService<MessageBusProduerService>();
            }

             
        }
    }
}
