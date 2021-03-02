using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Server.Endpoints
{
    public interface IMqttServerEndpoint : IDisposable
    {
        string Id { get; }

        Task OpenEndpointAsync(OpenEndpointContext context, CancellationToken cancellationToken);
    }
}
