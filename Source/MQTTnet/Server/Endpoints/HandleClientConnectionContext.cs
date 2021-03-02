using System;
using MQTTnet.Channel;

namespace MQTTnet.Server.Endpoints
{
    public sealed class HandleClientConnectionContext
    {
        public HandleClientConnectionContext(IMqttChannel channel)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }
        
        public IMqttChannel Channel { get; }
    }
}