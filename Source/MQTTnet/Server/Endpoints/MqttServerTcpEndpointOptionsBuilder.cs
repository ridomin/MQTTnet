using System;
using System.Net;
using System.Net.Sockets;
using MQTTnet.Server.Endpoints;

namespace MQTTnet.Server
{
    public sealed class MqttServerTcpEndpointOptionsBuilder
    {
        readonly MqttServerTcpEndpointOptions _options = new MqttServerTcpEndpointOptions();

        public MqttServerTcpEndpointOptionsBuilder()
        {
            WithDefaultUnencryptedPort();
        }
        
        public MqttServerTcpEndpointOptionsBuilder WithDefaultUnencryptedPort()
        {
            return WithPort(1883);
        }
        
        public MqttServerTcpEndpointOptionsBuilder WithDefaultEncryptedPort()
        {
            return WithPort(8883);
        }
        
        public MqttServerTcpEndpointOptionsBuilder WithPort(int value)
        {
            _options.Port = value;
            return this;
        }

        public MqttServerTcpEndpointOptionsBuilder WithAddressFamily(AddressFamily? value)
        {
            _options.AddressFamily = value;
            return this;
        }
        
        public MqttServerTcpEndpointOptionsBuilder WithBoundAddress(IPAddress value)
        {
            _options.BoundAddress = value;
            return this;
        }
        
        public MqttServerTcpEndpointOptionsBuilder WithConnectionBacklog(int value)
        {
            _options.ConnectionBacklog = value;
            return this;
        }
        
        public MqttServerTcpEndpointOptionsBuilder WithNoDelay(bool value = true)
        {
            _options.NoDelay = value;
            return this;
        }
        
        public MqttServerTcpEndpointOptionsBuilder WithReuseAddress(bool value = true)
        {
            _options.ReuseAddress = value;
            return this;
        }
        
        public MqttServerTcpEndpointOptionsBuilder WithTlsEncryption(Action<MqttServerTcpEndpointTlsOptionsBuilder> optionsBuilder)
        {
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));

            var optionsBuilderInstance = new MqttServerTcpEndpointTlsOptionsBuilder();
            optionsBuilder.Invoke(optionsBuilderInstance);

            _options.TlsOptions = optionsBuilderInstance.Build();
            return this;
        }
        
        public MqttServerTcpEndpointOptions Build()
        {
            return _options;
        }
    }
}