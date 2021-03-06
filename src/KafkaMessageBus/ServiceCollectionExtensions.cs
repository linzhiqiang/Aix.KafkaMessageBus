﻿using Aix.KafkaMessageBus.Impl;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.KafkaMessageBus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaMessageBus(this IServiceCollection services, KafkaMessageBusOptions options)
        {
            services
               .AddSingleton<KafkaMessageBusOptions>(options)
               .AddSingleton<IKafkaMessageBus, KafkaMessageBus>();

            return services;
        }
    }
}
