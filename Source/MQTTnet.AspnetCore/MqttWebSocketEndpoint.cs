using Microsoft.AspNetCore.Http;
using MQTTnet.Implementations;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Server.Endpoints;

namespace MQTTnet.AspNetCore
{
    public sealed class MqttWebSocketEndpoint : IMqttServerEndpoint
    {
        Func<HandleClientConnectionContext, Task> _clientConnectionHandler;
        
        public string Id { get; set; }
        
        public Task OpenEndpointAsync(OpenEndpointContext context, CancellationToken cancellationToken)
        {
            _clientConnectionHandler = context.ClientConnectionHandler;
            
            return Task.CompletedTask;
        }
       
        public async Task HandleWebSocketConnectionAsync(WebSocket webSocket, HttpContext httpContext)
        {
            if (webSocket == null) throw new ArgumentNullException(nameof(webSocket));

            var endpoint = $"{httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}";

            using (var clientCertificate = await httpContext.Connection.GetClientCertificateAsync().ConfigureAwait(false))
            {
                var clientConnectionHandler = _clientConnectionHandler;
                if (clientConnectionHandler == null)
                {
                    return;
                }

                var isSecureConnection = clientCertificate != null;
                    
                using (var mqttChannel = new MqttWebSocketChannel(webSocket, endpoint, isSecureConnection, clientCertificate))
                {
                    var context = new HandleClientConnectionContext(mqttChannel);
                    await clientConnectionHandler(context).ConfigureAwait(false);    
                }
            }
        }
        
        public void Dispose()
        {
        }
    }
}