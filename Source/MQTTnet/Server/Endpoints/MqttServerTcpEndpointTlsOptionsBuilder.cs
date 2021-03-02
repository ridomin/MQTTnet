using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using MQTTnet.Certificates;

namespace MQTTnet.Server.Endpoints
{
    public sealed class MqttServerTcpEndpointTlsOptionsBuilder
    {
        readonly MqttServerTcpEndpointTlsOptions _options = new MqttServerTcpEndpointTlsOptions();
        
        public MqttServerTcpEndpointTlsOptions Build()
        {
            return _options;
        }

        public MqttServerTcpEndpointTlsOptionsBuilder WithCertificateRevocationCheck()
        {
            return CheckCertificateRevocation(true);
        }
        
        public MqttServerTcpEndpointTlsOptionsBuilder WithoutCertificateRevocationCheck()
        {
            return CheckCertificateRevocation(false);
        }
        
        public MqttServerTcpEndpointTlsOptionsBuilder CheckCertificateRevocation(bool value)
        {
            _options.CheckCertificateRevocation = value;
            return this;
        }

        public MqttServerTcpEndpointTlsOptionsBuilder WithProtocolVersion(SslProtocols value)
        {
            _options.SslProtocol = value;
            return this;
        }

        public MqttServerTcpEndpointTlsOptionsBuilder WithEncryptionCertificate(byte[] buffer, IMqttServerCertificateCredentials credentials = null)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            
            _options.CertificateProvider = new BlobCertificateProvider(buffer)
            {
                Password = credentials?.Password
            };
            
            return this;
        }
        
        public MqttServerTcpEndpointTlsOptionsBuilder WithEncryptionCertificate(X509Certificate2 certificate)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            
            _options.CertificateProvider = new X509CertificateProvider(certificate);
            
            return this;
        }
    }
}