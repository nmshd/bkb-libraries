using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Enmeshed.BuildingBlocks.Application.Abstractions.Infrastructure.EventBus;
using Enmeshed.BuildingBlocks.Application.Abstractions.Infrastructure.EventBus.Events;
using Enmeshed.BuildingBlocks.Infrastructure.EventBus.Json;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enmeshed.BuildingBlocks.Infrastructure.EventBus.AzureServiceBus
{
    public class EventBusAzureServiceBus : IEventBus
    {
        private const string INTEGRATION_EVENT_SUFFIX = "IntegrationEvent";
        private const string AUTOFAC_SCOPE_NAME = "event_bus";

        private readonly ILifetimeScope _autofac;
        private readonly ILogger<EventBusAzureServiceBus> _logger;
        private readonly IServiceBusPersisterConnection _serviceBusPersisterConnection;
        private readonly SubscriptionClient _subscriptionClient;
        private readonly IEventBusSubscriptionsManager _subsManager;

        public EventBusAzureServiceBus(IServiceBusPersisterConnection serviceBusPersisterConnection,
            ILogger<EventBusAzureServiceBus> logger, IEventBusSubscriptionsManager subsManager,
            string subscriptionClientName, ILifetimeScope autofac)
        {
            _serviceBusPersisterConnection = serviceBusPersisterConnection;
            _logger = logger;
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();

            _subscriptionClient = new SubscriptionClient(
                serviceBusPersisterConnection.ServiceBusConnectionStringBuilder,
                subscriptionClientName);
            _autofac = autofac;

            RemoveDefaultRule().GetAwaiter().GetResult();
            RegisterSubscriptionClientMessageHandler();
        }

        public async void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
            {
                ContractResolver = new ContractResolverWithPrivates()
            });
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new Message
            {
                MessageId = @event.IntegrationEventId,
                Body = body,
                Label = eventName
            };
            var topicClient = _serviceBusPersisterConnection.CreateModel();

            _logger.LogTrace($"Sending integration event with id '{message.MessageId}'...");

            await topicClient.SendAsync(message);

            _logger.LogTrace($"Successfully sent integration event with id '{message.MessageId}'.");
        }

        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.AddDynamicSubscription<TH>(eventName);
        }

        public async void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

            var containsKey = _subsManager.HasSubscriptionsForEvent<T>();
            if (!containsKey)
                try
                {
                    _logger.LogTrace("Trying to subscribe to subscription client...");

                    await _subscriptionClient.AddRuleAsync(new RuleDescription
                    {
                        Filter = new CorrelationFilter {Label = eventName},
                        Name = eventName
                    });

                    _logger.LogTrace("Successfully subscribed to subscription client.");
                }
                catch (ServiceBusException)
                {
                    _logger.LogInformation($"The messaging entity {eventName} already exists.");
                }

            _subsManager.AddSubscription<T, TH>();
        }

        public async void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

            try
            {
                _logger.LogTrace("Trying to unsubscribe from subscription client...");

                await _subscriptionClient.RemoveRuleAsync(eventName);

                _logger.LogTrace("Successfully unsubscribed from subscription client.");
            }
            catch (MessagingEntityNotFoundException)
            {
                _logger.LogInformation($"The messaging entity {eventName} Could not be found.");
            }

            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            _subsManager.Clear();
        }

        private void RegisterSubscriptionClientMessageHandler()
        {
            _logger.LogTrace("Trying to register message handler...");

            _subscriptionClient.RegisterMessageHandler(
                async (message, _) =>
                {
                    var eventName = $"{message.Label}{INTEGRATION_EVENT_SUFFIX}";
                    var messageData = Encoding.UTF8.GetString(message.Body);
                    try
                    {
                        await ProcessEvent(eventName, messageData);

                        // Complete the message so that it is not received again.
                        await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            $"An error occurred while processing the integration event with id '{message.MessageId}'.");
                    }

                    _logger.LogTrace("Successfully registered message handler.");
                },
                new MessageHandlerOptions(ExceptionReceivedHandler) {MaxConcurrentCalls = 10, AutoComplete = false});
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace($"Received integration event '{eventName}'.");

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                await using var scope = _autofac.BeginLifetimeScope(AUTOFAC_SCOPE_NAME);

                var subscriptions = _subsManager.GetHandlersForEvent(eventName).ToArray();

                _logger.LogTrace($"Found {subscriptions.Length} subscriptions for '{eventName}' event.");

                foreach (var subscription in subscriptions)
                    if (subscription.IsDynamic)
                    {
                        var handler =
                            scope.ResolveOptional(subscription.HandlerType) as IDynamicIntegrationEventHandler;
                        if (handler == null)
                            throw new Exception(
                                $"The handler type {subscription.HandlerType.FullName} is not registered in the dependency container.");

                        dynamic eventData = JObject.Parse(message);
                        await handler.Handle(eventData);

                        _logger.LogTrace("Successfully invoked dynamic integration event handler...");
                    }
                    else
                    {
                        var eventType = _subsManager.GetEventTypeByName(eventName);

                        if (eventType == null) throw new Exception($"Unsupported event type '${eventType}' received.");

                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType,
                            new JsonSerializerSettings
                            {
                                ContractResolver = new ContractResolverWithPrivates()
                            });
                        var handler = scope.ResolveOptional(subscription.HandlerType);
                        if (handler == null)
                            throw new Exception(
                                $"The handler type {subscription.HandlerType.FullName} is not registered in the dependency container.");

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task) concreteType.GetMethod("Handle")!.Invoke(handler, new[] {integrationEvent})!;

                        _logger.LogTrace("Successfully invoked integration event handler...");
                    }
            }
        }

        private async Task RemoveDefaultRule()
        {
            try
            {
                await _subscriptionClient.RemoveRuleAsync(RuleDescription.DefaultRuleName);
            }
            catch (MessagingEntityNotFoundException)
            {
                _logger.LogInformation($"The messaging entity {RuleDescription.DefaultRuleName} Could not be found.");
            }
        }
    }
}