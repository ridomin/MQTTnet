using MQTTnet.Adapter;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Receiving;
using MQTTnet.Diagnostics;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using MQTTnet.Server.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Formatter;
using MQTTnet.Implementations;
using MQTTnet.Server.Endpoints;

namespace MQTTnet.Server
{
    public sealed class MqttServer : IMqttServer
    {
        readonly MqttServerEventDispatcher _eventDispatcher;
        readonly IMqttNetLogger _rootLogger;
        readonly IMqttNetScopedLogger _logger;

        MqttClientSessionsManager _clientSessionsManager;
        IMqttRetainedMessagesManager _retainedMessagesManager;
        MqttServerKeepAliveMonitor _keepAliveMonitor;
        CancellationTokenSource _cancellationToken;
        bool _isDisposed;

        public MqttServer(IMqttNetLogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger.CreateScopedLogger(nameof(MqttServer));
            _rootLogger = logger;

            _eventDispatcher = new MqttServerEventDispatcher(logger);
        }

        public bool IsStarted => _cancellationToken != null;

        public IMqttServerStartedHandler StartedHandler { get; set; }

        public IMqttServerStoppedHandler StoppedHandler { get; set; }

        public IMqttServerClientConnectedHandler ClientConnectedHandler
        {
            get => _eventDispatcher.ClientConnectedHandler;
            set => _eventDispatcher.ClientConnectedHandler = value;
        }

        public IMqttServerClientDisconnectedHandler ClientDisconnectedHandler
        {
            get => _eventDispatcher.ClientDisconnectedHandler;
            set => _eventDispatcher.ClientDisconnectedHandler = value;
        }

        public IMqttServerClientSubscribedTopicHandler ClientSubscribedTopicHandler
        {
            get => _eventDispatcher.ClientSubscribedTopicHandler;
            set => _eventDispatcher.ClientSubscribedTopicHandler = value;
        }

        public IMqttServerClientUnsubscribedTopicHandler ClientUnsubscribedTopicHandler
        {
            get => _eventDispatcher.ClientUnsubscribedTopicHandler;
            set => _eventDispatcher.ClientUnsubscribedTopicHandler = value;
        }

        public IMqttApplicationMessageReceivedHandler ApplicationMessageReceivedHandler
        {
            get => _eventDispatcher.ApplicationMessageReceivedHandler;
            set => _eventDispatcher.ApplicationMessageReceivedHandler = value;
        }

        public IMqttServerOptions Options { get; private set; }

        public Task<IList<IMqttClientStatus>> GetClientStatusAsync()
        {
            ThrowIfDisposed();
            ThrowIfNotStarted();

            return _clientSessionsManager.GetClientStatusAsync();
        }

        public Task<IList<IMqttSessionStatus>> GetSessionStatusAsync()
        {
            ThrowIfDisposed();
            ThrowIfNotStarted();

            return _clientSessionsManager.GetSessionStatusAsync();
        }

        public Task<IList<MqttApplicationMessage>> GetRetainedApplicationMessagesAsync()
        {
            ThrowIfDisposed();
            ThrowIfNotStarted();

            return _retainedMessagesManager.GetMessagesAsync();
        }

        public Task ClearRetainedApplicationMessagesAsync()
        {
            ThrowIfDisposed();
            ThrowIfNotStarted();

            return _retainedMessagesManager?.ClearMessagesAsync() ?? PlatformAbstractionLayer.CompletedTask;
        }

        public Task SubscribeAsync(string clientId, ICollection<MqttTopicFilter> topicFilters)
        {
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));
            if (topicFilters == null) throw new ArgumentNullException(nameof(topicFilters));
            
            ThrowIfDisposed();
            ThrowIfNotStarted();

            return _clientSessionsManager.SubscribeAsync(clientId, topicFilters);
        }

        public Task UnsubscribeAsync(string clientId, ICollection<string> topicFilters)
        {
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));
            if (topicFilters == null) throw new ArgumentNullException(nameof(topicFilters));

            ThrowIfDisposed();
            ThrowIfNotStarted();

            return _clientSessionsManager.UnsubscribeAsync(clientId, topicFilters);
        }

        public Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken)
        {
            if (applicationMessage == null) throw new ArgumentNullException(nameof(applicationMessage));

            ThrowIfDisposed();

            MqttTopicValidator.ThrowIfInvalid(applicationMessage.Topic);

            ThrowIfNotStarted();

            _clientSessionsManager.DispatchApplicationMessage(applicationMessage, null);

            return Task.FromResult(new MqttClientPublishResult());
        }

        public async Task StartAsync(IMqttServerOptions options)
        {
            ThrowIfDisposed();
            ThrowIfStarted();

            Options = options ?? throw new ArgumentNullException(nameof(options));
            
            _cancellationToken = new CancellationTokenSource();
            var cancellationToken = _cancellationToken.Token;

            _retainedMessagesManager = Options.RetainedMessagesManager ?? throw new MqttConfigurationException("options.RetainedMessagesManager must not be null.");
            await _retainedMessagesManager.Start(Options, _rootLogger).ConfigureAwait(false);
            await _retainedMessagesManager.LoadMessagesAsync().ConfigureAwait(false);

            _clientSessionsManager = new MqttClientSessionsManager(Options, _retainedMessagesManager, _eventDispatcher, _rootLogger);
            _clientSessionsManager.Start(cancellationToken);

            _keepAliveMonitor = new MqttServerKeepAliveMonitor(Options, _clientSessionsManager, _rootLogger);
            _keepAliveMonitor.Start(cancellationToken);

            await OpenEndpointsAsync(cancellationToken).ConfigureAwait(false);

            _logger.Info("Started.");

            var startedHandler = StartedHandler;
            if (startedHandler != null)
            {
                await startedHandler.HandleServerStartedAsync(EventArgs.Empty).ConfigureAwait(false);
            }
        }

        public async Task StopAsync()
        {
            try
            {
                if (_cancellationToken == null)
                {
                    return;
                }

                _cancellationToken.Cancel(false);

                await _clientSessionsManager.CloseAllConnectionsAsync().ConfigureAwait(false);
                
                foreach (var endpoint in Options.Endpoints)
                {
                    endpoint.Dispose();
                }
                
                _logger.Info("Stopped.");
            }
            finally
            {
                _clientSessionsManager?.Dispose();
                _clientSessionsManager = null;

                _cancellationToken?.Dispose();
                _cancellationToken = null;

                _retainedMessagesManager = null;
            }

            var stoppedHandler = StoppedHandler;
            if (stoppedHandler != null)
            {
                await stoppedHandler.HandleServerStoppedAsync(EventArgs.Empty).ConfigureAwait(false);
            }
        }
        
        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            
            _clientSessionsManager?.Dispose();
            _cancellationToken?.Dispose();

            _isDisposed = true;
        }
        
        async Task OpenEndpointsAsync(CancellationToken cancellationToken)
        {
            if (Options?.Endpoints?.Any() == false)
            {
                throw new InvalidOperationException("At least one endpoint must be configured in the options.");
            }

            _logger.Verbose("Opening endpoints.");

            foreach (var endpoint in Options.Endpoints)
            {
                var context = new OpenEndpointContext(HandleClientConnectionAsync, Options, _rootLogger);
                await endpoint.OpenEndpointAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }

        async Task HandleClientConnectionAsync(HandleClientConnectionContext handleClientConnectionContext)
        {
            using (var channelAdapter = new MqttChannelAdapter(handleClientConnectionContext.Channel, new MqttPacketFormatterAdapter(new MqttPacketWriter()), null, _rootLogger))
            {
                await _clientSessionsManager.HandleClientConnectionAsync(channelAdapter, handleClientConnectionContext, CancellationToken.None).ConfigureAwait(false);    
            }
        }

        void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(MqttServer));
            }
        }
        
        void ThrowIfStarted()
        {
            if (_cancellationToken != null)
            {
                throw new InvalidOperationException("The MQTT server is already started.");
            }
        }

        void ThrowIfNotStarted()
        {
            if (_cancellationToken == null)
            {
                throw new InvalidOperationException("The MQTT server is not started.");
            }
        }
    }
}
