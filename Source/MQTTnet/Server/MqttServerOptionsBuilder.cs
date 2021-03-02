using System;
using System.Threading.Tasks;
using MQTTnet.Server.Endpoints;

namespace MQTTnet.Server
{
    public class MqttServerOptionsBuilder
    {
        readonly MqttServerOptions _options = new MqttServerOptions();
        
        public MqttServerOptionsBuilder WithDefaultTcpEndpoint(int port = 1883)
        {
            return WithTcpEndpoint("default", o =>
            {
                o.WithPort(port);
            });
        }
        
        public MqttServerOptionsBuilder WithEndpoint(IMqttServerEndpoint endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            
            _options.Endpoints.Add(endpoint);
            return this;
        }
        
        public MqttServerOptionsBuilder WithTcpEndpoint(string id, Action<MqttServerTcpEndpointOptionsBuilder> optionsBuilder)
        {
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));
            
            var endpointOptionsBuilderInstance = new MqttServerTcpEndpointOptionsBuilder();
            optionsBuilder.Invoke(endpointOptionsBuilderInstance);

            var endpoint = new MqttServerTcpEndpoint(id, endpointOptionsBuilderInstance.Build());

            _options.Endpoints.Add(endpoint);
            
            return this;
        }
        
        public MqttServerOptionsBuilder WithMaxPendingMessagesPerClient(int value)
        {
            _options.MaxPendingMessagesPerClient = value;
            return this;
        }

        public MqttServerOptionsBuilder WithDefaultCommunicationTimeout(TimeSpan value)
        {
            _options.DefaultCommunicationTimeout = value;
            return this;
        }
        
        public MqttServerOptionsBuilder WithStorage(IMqttServerStorage value)
        {
            _options.Storage = value;
            return this;
        }

        public MqttServerOptionsBuilder WithRetainedMessagesManager(IMqttRetainedMessagesManager value)
        {
            _options.RetainedMessagesManager = value;
            return this;
        }

        public MqttServerOptionsBuilder WithConnectionValidator(IMqttServerConnectionValidator value)
        {
            _options.ConnectionValidator = value;
            return this;
        }

        public MqttServerOptionsBuilder WithConnectionValidator(Action<MqttConnectionValidatorContext> value)
        {
            _options.ConnectionValidator = new MqttServerConnectionValidatorDelegate(value);
            return this;
        }

        public MqttServerOptionsBuilder WithDisconnectedInterceptor(IMqttServerClientDisconnectedHandler value)
        {
            _options.ClientDisconnectedInterceptor = value;
            return this;
        }

        public MqttServerOptionsBuilder WithDisconnectedInterceptor(Action<MqttServerClientDisconnectedEventArgs> value)
        {
            _options.ClientDisconnectedInterceptor = new MqttServerClientDisconnectedHandlerDelegate(value);
            return this;
        }

        public MqttServerOptionsBuilder WithApplicationMessageInterceptor(IMqttServerApplicationMessageInterceptor value)
        {
            _options.ApplicationMessageInterceptor = value;
            return this;
        }

        public MqttServerOptionsBuilder WithApplicationMessageInterceptor(Action<MqttApplicationMessageInterceptorContext> value)
        {
            _options.ApplicationMessageInterceptor = new MqttServerApplicationMessageInterceptorDelegate(value);
            return this;
        }

        public MqttServerOptionsBuilder WithApplicationMessageInterceptor(Func<MqttApplicationMessageInterceptorContext, Task> value)
        {
            _options.ApplicationMessageInterceptor = new MqttServerApplicationMessageInterceptorDelegate(value);
            return this;
        }

        public MqttServerOptionsBuilder WithMultiThreadedApplicationMessageInterceptor(Action<MqttApplicationMessageInterceptorContext> value)
        {
            _options.ApplicationMessageInterceptor = new MqttServerMultiThreadedApplicationMessageInterceptorDelegate(value);
            return this;
        }

        public MqttServerOptionsBuilder WithMultiThreadedApplicationMessageInterceptor(Func<MqttApplicationMessageInterceptorContext, Task> value)
        {
            _options.ApplicationMessageInterceptor = new MqttServerMultiThreadedApplicationMessageInterceptorDelegate(value);
            return this;
        }

        public MqttServerOptionsBuilder WithClientMessageQueueInterceptor(IMqttServerClientMessageQueueInterceptor value)
        {
            _options.ClientMessageQueueInterceptor = value;
            return this;
        }

        public MqttServerOptionsBuilder WithClientMessageQueueInterceptor(Action<MqttClientMessageQueueInterceptorContext> value)
        {
            _options.ClientMessageQueueInterceptor = new MqttClientMessageQueueInterceptorDelegate(value);
            return this;
        }

        public MqttServerOptionsBuilder WithSubscriptionInterceptor(IMqttServerSubscriptionInterceptor value)
        {
            _options.SubscriptionInterceptor = value;
            return this;
        }

        public MqttServerOptionsBuilder WithUnsubscriptionInterceptor(IMqttServerUnsubscriptionInterceptor value)
        {
            _options.UnsubscriptionInterceptor = value;
            return this;
        }

        public MqttServerOptionsBuilder WithSubscriptionInterceptor(Action<MqttSubscriptionInterceptorContext> value)
        {
            _options.SubscriptionInterceptor = new MqttServerSubscriptionInterceptorDelegate(value);
            return this;
        }

        public MqttServerOptionsBuilder WithUndeliveredMessageInterceptor(Action<MqttApplicationMessageInterceptorContext> value)
        {
            _options.UndeliveredMessageInterceptor = new MqttServerApplicationMessageInterceptorDelegate(value);
            return this;
        }

        public MqttServerOptionsBuilder WithPersistentSessions()
        {
            _options.EnablePersistentSessions = true;
            return this;
        }

        /// <summary>
        /// Gets or sets the client ID which is used when publishing messages from the server directly.
        /// </summary>
        public MqttServerOptionsBuilder WithClientId(string value)
        {
            _options.ClientId = value;
            return this;
        }

        public IMqttServerOptions Build()
        {
            return _options;
        }
    }
}
