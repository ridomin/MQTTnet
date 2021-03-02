using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MQTTnet.Diagnostics;
using MQTTnet.Internal;
using MQTTnet.Server;

namespace MQTTnet.AspNetCore
{
    public sealed class MqttHostedServer : IHostedService
    {
        readonly IMqttServer _mqttServer;
        readonly IMqttServerOptions _options;

        public MqttHostedServer(IMqttServerOptions options, IMqttNetLogger logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _mqttServer = new MqttServer(logger);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _mqttServer.StartAsync(_options).RunInBackground();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _mqttServer.StopAsync();
        }
    }
}
