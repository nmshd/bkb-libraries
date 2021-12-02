using System;
using Autofac;
using Enmeshed.BuildingBlocks.Application.Abstractions.Infrastructure.EventBus;
using Enmeshed.BuildingBlocks.Infrastructure.EventBus;
using Enmeshed.BuildingBlocks.Infrastructure.EventBus.AzureServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureServiceBusServiceCollectionExtensions
    {
        public static void AddAzureServiceBus(this IServiceCollection services, Action<ServiceBusOptions> setupOptions)
        {
            var options = new ServiceBusOptions();
            setupOptions.Invoke(options);

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            services.AddSingleton<IServiceBusPersisterConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersisterConnection>>();

                var serviceBusConnection = new ServiceBusConnectionStringBuilder(options.ConnectionString);

                return new DefaultServiceBusPersisterConnection(serviceBusConnection, logger);
            });

            services.AddSingleton<IEventBus, EventBusAzureServiceBus>(sp =>
            {
                var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusAzureServiceBus>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                return new EventBusAzureServiceBus(serviceBusPersisterConnection, logger, eventBusSubcriptionsManager,
                    options.SubscriptionClientName, iLifetimeScope);
            });
        }
    }

    public class ServiceBusOptions
    {
#pragma warning disable CS8618
        public string ConnectionString { get; set; }
        public string SubscriptionClientName { get; set; }
#pragma warning restore CS8618
    }
}