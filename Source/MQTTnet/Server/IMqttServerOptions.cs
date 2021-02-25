using System;
using System.Collections.Generic;
using MQTTnet.Server.Endpoints;

namespace MQTTnet.Server
{
    public interface IMqttServerOptions
    {
        List<IMqttServerEndpoint> Endpoints { get; }
        
        /// <summary>
        /// Gets the client identifier.
        /// Hint: This identifier needs to be unique over all used clients / devices on the broker to avoid connection issues.
        /// </summary>
        string ClientId { get; set; }

        bool EnablePersistentSessions { get; }

        int MaxPendingMessagesPerClient { get; }
        MqttPendingMessagesOverflowStrategy PendingMessagesOverflowStrategy { get; }

        TimeSpan DefaultCommunicationTimeout { get; }
        TimeSpan KeepAliveMonitorInterval { get; }

        IMqttServerConnectionValidator ConnectionValidator { get; }
        IMqttServerSubscriptionInterceptor SubscriptionInterceptor { get; }
        IMqttServerUnsubscriptionInterceptor UnsubscriptionInterceptor { get; }
        IMqttServerApplicationMessageInterceptor ApplicationMessageInterceptor { get; }
        IMqttServerClientMessageQueueInterceptor ClientMessageQueueInterceptor { get; }

        [Obsolete("Please use _Endpoints_ instead. This will be removed soon.")]
        MqttServerTcpEndpointOptions DefaultEndpointOptions { get; }

        [Obsolete("Please use _Endpoints_ instead. This will be removed soon.")]
        MqttServerTlsTcpEndpointOptions TlsEndpointOptions { get; }

        IMqttServerStorage Storage { get; }

        IMqttRetainedMessagesManager RetainedMessagesManager { get; }

        IMqttServerApplicationMessageInterceptor UndeliveredMessageInterceptor { get; set; }

        IMqttServerClientDisconnectedHandler ClientDisconnectedInterceptor { get; set; }
    }
}