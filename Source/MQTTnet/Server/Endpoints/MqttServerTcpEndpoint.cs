#if !WINDOWS_UWP
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Diagnostics;
using MQTTnet.Implementations;
using MQTTnet.Internal;

namespace MQTTnet.Server.Endpoints
{
    public sealed class MqttServerTcpEndpoint : IMqttServerEndpoint
    {
        readonly MqttServerTcpEndpointOptions _options;
        
        CrossPlatformSocket _listenerSocket;
        IMqttNetScopedLogger _logger;
        IPEndPoint _localEndPoint;
        
        CancellationTokenSource _cancellationToken;
        Func<HandleClientConnectionContext, Task> _clientConnectionHandler;

        public MqttServerTcpEndpoint(string id, MqttServerTcpEndpointOptions options)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        
        public string Id { get; }
        
        public Task OpenEndpointAsync(OpenEndpointContext context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _logger = context.Logger.CreateScopedLogger(nameof(MqttServerTcpEndpoint));
         
            Dispose();

            _clientConnectionHandler = context.ClientConnectionHandler;
            
            _cancellationToken = new CancellationTokenSource();
            
            _localEndPoint = new IPEndPoint(_options.BoundAddress, _options.Port);

            _listenerSocket = !_options.AddressFamily.HasValue ? new CrossPlatformSocket() : new CrossPlatformSocket(_options.AddressFamily.Value);
            _listenerSocket.Bind(_localEndPoint);
            _listenerSocket.NoDelay = _options.NoDelay;
            _listenerSocket.ReuseAddress = _options.ReuseAddress;
            _listenerSocket.Listen(_options.ConnectionBacklog);

            _logger.Info($"Starting TCP listener for {_localEndPoint} TLS={_options.TlsOptions != null}.");

            Task.Run(() => AcceptClientsAsync(_cancellationToken.Token), _cancellationToken.Token).RunInBackground();

            return PlatformAbstractionLayer.CompletedTask;
        }

        async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var clientSocket = await _listenerSocket.AcceptAsync().ConfigureAwait(false);
                if (clientSocket == null)
                {
                    continue;
                }

                Task.Run(() => AcceptClientAsync(clientSocket, cancellationToken), cancellationToken).RunInBackground();
            }
        }

        async Task AcceptClientAsync(CrossPlatformSocket clientSocket, CancellationToken cancellationToken)
        {
            Stream networkStream = null;
            SslStream sslStream = null;
            X509Certificate2 clientCertificate = null;

            try
            {
                clientSocket.NoDelay = _options.NoDelay;

                var remoteEndPoint = clientSocket.RemoteEndPoint.ToString();
                networkStream = clientSocket.GetStream();
                
                var tlsOptions = _options.TlsOptions;
                var tlsCertificate = tlsOptions?.CertificateProvider?.GetCertificate();
                if (tlsCertificate != null)
                {
                    sslStream = new SslStream(networkStream, false, tlsOptions.RemoteCertificateValidationCallback);

                    await sslStream.AuthenticateAsServerAsync(
                        tlsCertificate,
                        tlsOptions.ClientCertificateRequired,
                        tlsOptions.SslProtocol,
                        tlsOptions.CheckCertificateRevocation).ConfigureAwait(false);

                    networkStream = sslStream;

                    clientCertificate = sslStream.RemoteCertificate as X509Certificate2;

                    if (clientCertificate == null && sslStream.RemoteCertificate != null)
                    {
                        clientCertificate = new X509Certificate2(sslStream.RemoteCertificate.Export(X509ContentType.Cert));
                    }
                }

                var channel = new MqttTcpChannel(networkStream, remoteEndPoint, clientCertificate);
                var context = new HandleClientConnectionContext(channel);
                await _clientConnectionHandler.Invoke(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                sslStream?.Dispose();
                networkStream?.Dispose();

#if NETSTANDARD1_3 || NETSTANDARD2_0 || NET461 || NET472
                clientCertificate?.Dispose();
#endif

                if (exception is SocketException socketException)
                {
                    if (socketException.SocketErrorCode == SocketError.ConnectionAborted ||
                        socketException.SocketErrorCode == SocketError.OperationAborted)
                    {
                        return;
                    }
                }

                _logger.Error(exception, $"Error while accepting connection at TCP listener {_localEndPoint} TLS={_options.TlsOptions != null}.");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _cancellationToken?.Cancel();
            _cancellationToken?.Dispose();
            _cancellationToken = null;
            
            _listenerSocket?.Dispose();
        }
    }
}
#endif