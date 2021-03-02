using System;
using System.Threading.Tasks;
using MQTTnet.Diagnostics;

namespace MQTTnet.Server.Endpoints
{
    public sealed class OpenEndpointContext
    {
        public OpenEndpointContext(Func<HandleClientConnectionContext, Task> clientConnectionHandler, IMqttServerOptions serverOptions, IMqttNetLogger logger)
        {
            ClientConnectionHandler = clientConnectionHandler ?? throw new ArgumentNullException(nameof(clientConnectionHandler));
            ServerOptions = serverOptions ?? throw new ArgumentNullException(nameof(serverOptions));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public Func<HandleClientConnectionContext, Task> ClientConnectionHandler { get; }
        
        public IMqttServerOptions ServerOptions { get; }
        
        public IMqttNetLogger Logger { get; }
    }
}