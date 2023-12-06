using MQTTnet.Adapter;
using MQTTnet.Channel;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Formatter;
using MQTTnet.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubAckSamples
{
    internal class MyChannelFactory : IMqttClientAdapterFactory
    {
        Func<int, bool>? _dropPackageCallback;
        public MyChannelFactory(Func<int, bool>? dropPackageCallback)
        {
            _dropPackageCallback = dropPackageCallback;
        }

        public IMqttChannelAdapter CreateClientAdapter(MqttClientOptions options, MqttPacketInspector packetInspector, IMqttNetLogger logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            IMqttChannel channel;
            switch (options.ChannelOptions)
            {
                case MqttClientTcpOptions _:
                    {
                        channel = new MqttTcpChannel(options);
                        break;
                    }

                case MqttClientWebSocketOptions webSocketOptions:
                    {
                        channel = new MqttWebSocketChannel(webSocketOptions);
                        break;
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }

            var bufferWriter = new MqttBufferWriter(options.WriterBufferSize, options.WriterBufferSizeMax);
            var packetFormatterAdapter = new MqttPacketFormatterAdapter(options.ProtocolVersion, bufferWriter);

            return new MyMqttChannelAdapter(channel, packetFormatterAdapter, logger, _dropPackageCallback)
            {
                AllowPacketFragmentation = options.AllowPacketFragmentation,
                PacketInspector = packetInspector
            };
        }
    }
}
