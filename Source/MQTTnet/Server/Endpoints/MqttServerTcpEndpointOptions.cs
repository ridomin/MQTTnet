using System.Net;
using System.Net.Sockets;

namespace MQTTnet.Server.Endpoints
{
    public sealed class MqttServerTcpEndpointOptions
    {
        public int Port { get; set; } = 1883;

        public int ConnectionBacklog { get; set; } = 10;

        public bool NoDelay { get; set; } = true;
        
        public IPAddress BoundAddress { get; set; } = IPAddress.Any;

        /// <summary>
        /// This requires admin permissions on Linux.
        /// </summary>
        public bool ReuseAddress { get; set; }

        /// <summary>
        /// Leaving this _null_ will use a dual mode socket.
        /// </summary>
        public AddressFamily? AddressFamily { get; set; }

        public MqttServerTcpEndpointTlsOptions TlsOptions { get; set; }
    }
}