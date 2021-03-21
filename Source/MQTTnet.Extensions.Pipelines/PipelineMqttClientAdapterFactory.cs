using System;
using System.Net;
using MQTTnet.Adapter;
using MQTTnet.AspNetCore;
using MQTTnet.Client.Options;
using MQTTnet.Formatter;

namespace MQTTnet.Extensions.Pipelines
{
    public sealed class PipelineMqttClientAdapterFactory : IMqttClientAdapterFactory
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

                        var writer = new SpanBasedMqttPacketWriter();
                        var formatter = new MqttPacketFormatterAdapter(options.ProtocolVersion, writer);
                        return new MqttConnectionContext(formatter, tcpConnection);
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
