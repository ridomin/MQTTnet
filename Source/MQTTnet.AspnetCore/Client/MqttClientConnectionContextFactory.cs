using MQTTnet.Adapter;
using MQTTnet.AspNetCore.Client.Tcp;
using MQTTnet.Client.Options;
using MQTTnet.Formatter;
using System;
using System.Net;

namespace MQTTnet.AspNetCore.Client
{
    public class MqttClientConnectionContextFactory : IMqttClientAdapterFactory
    {
        public IMqttChannelAdapter CreateClientAdapter(IMqttClientOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            switch (options.ChannelOptions)
            {
                case MqttClientTcpOptions tcpOptions:
                    {
                        var endpoint = new DnsEndPoint(tcpOptions.Server, tcpOptions.GetPort());
                        var tcpConnection = new TcpConnection(endpoint);

                        var packetWriter = new SpanBasedMqttPacketWriter();
                        var packetFormatter = new MqttPacketFormatterAdapter(options.ProtocolVersion, packetWriter);
                        
                        return new MqttConnectionContext(packetFormatter, tcpConnection);
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
