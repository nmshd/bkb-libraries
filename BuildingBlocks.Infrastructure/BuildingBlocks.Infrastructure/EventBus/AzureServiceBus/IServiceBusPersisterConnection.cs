using System;
using Microsoft.Azure.ServiceBus;

namespace Enmeshed.BuildingBlocks.Infrastructure.EventBus.AzureServiceBus
{
    public interface IServiceBusPersisterConnection : IDisposable
    {
        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder { get; }

        ITopicClient CreateModel();
    }
}