using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Adapter;
using MQTTnet.Channel;
using MQTTnet.Diagnostics;
using MQTTnet.Formatter;
using MQTTnet.Internal;

namespace MQTTnet.Server.Endpoints
{
    public sealed class MqttServerEndpointAdapter : IDisposable
    {
        CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        
        readonly IMqttServerEndpoint _endpoint;
        readonly Func<IMqttChannelAdapter, Task> _clientHandlerCallback;
        readonly IMqttServerOptions _options;
        readonly IMqttNetScopedLogger _logger;
        readonly IMqttNetLogger _rootLogger;
        
        public MqttServerEndpointAdapter(
            IMqttServerEndpoint endpoint,
            Func<IMqttChannelAdapter, Task> clientHandlerCallback,
            IMqttServerOptions options,
            IMqttNetLogger logger)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _clientHandlerCallback = clientHandlerCallback ?? throw new ArgumentNullException(nameof(clientHandlerCallback));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _rootLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger = logger.CreateScopedLogger(nameof(MqttServerEndpointAdapter));
        }

        public void Start()
        {
            Task.Run(() => DoWork(_cancellationToken.Token), _cancellationToken.Token);
        }

        async Task DoWork(CancellationToken cancellationToken)
        {
            try
            {
                // var openEndpointContext = new OpenEndpointContext
                // {
                //     ServerOptions = _options,
                //     Logger = _rootLogger
                // };
                //
                // await _endpoint.OpenEndpointAsync(openEndpointContext, cancellationToken).ConfigureAwait(false);

                while (!cancellationToken.IsCancellationRequested)
                {
                    await TryAcceptClientAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                _endpoint.Dispose();
            }
            
        }

        async Task TryAcceptClientAsync(CancellationToken cancellationToken)
        {
            try
            {
                //var context = new HandleClientConnectionContext();
                
                //var client = await _endpoint.AcceptClientAsync(context, cancellationToken).ConfigureAwait(false);
                // if (client == null)
                // {
                //     return;
                // }

                //Task.Run(() => TryHandleConnectionAsync(client, cancellationToken), cancellationToken).RunInBackground(_logger);
            }
            catch (Exception exception)
            {
                
            }
        }

        async Task TryHandleConnectionAsync(IMqttChannel channel, CancellationToken cancellationToken)
        {
            try
            {
                using (var clientAdapter = new MqttChannelAdapter(channel, new MqttPacketFormatterAdapter(new MqttPacketWriter()), null, _rootLogger))
                {
                    await _clientHandlerCallback(clientAdapter).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                if (exception is ObjectDisposedException)
                {
                    // It can happen that the listener socket is accessed after the cancellation token is already set and the listener socket is disposed.
                    return;
                }

                if (exception is SocketException socketException &&
                    socketException.SocketErrorCode == SocketError.OperationAborted)
                {
                    return;
                }

                _logger.Error(exception, "Error while handling client connection.");
            }
        }

        public void Dispose()
        {
            try
            {
                _cancellationToken?.Cancel();
                _cancellationToken?.Dispose();
                _cancellationToken = null;
            }
            finally
            {
                _endpoint.Dispose();
            }
        }
    }
}
