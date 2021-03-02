using System;
using MQTTnet.Channel;

namespace MQTTnet.Server.Endpoints
{
    public sealed class HandleClientConnectionContext
    {
        public HandleClientConnectionContext(IMqttChannel channel, IMqttServerEndpoint originEndpoint)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            OriginEndpoint = originEndpoint ?? throw new ArgumentNullException(nameof(originEndpoint));
        }
        
        public IMqttChannel Channel { get; }

        public IMqttServerEndpoint OriginEndpoint { get; }
    }
}