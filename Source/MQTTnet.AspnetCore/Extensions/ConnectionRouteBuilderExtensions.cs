using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;

namespace MQTTnet.AspNetCore.Extensions
{
    public static class ConnectionRouteBuilderExtensions
    {
#if NETCOREAPP2_1 || NETSTANDARD
        public static void MapMqtt(this ConnectionsRouteBuilder connection, PathString path)
        {
            connection.MapConnectionHandler<MqttConnectionHandler>(path, options =>
            {
                options.WebSockets.SubProtocolSelector = MqttSubProtocolSelector.SelectSubProtocol;
            });
        }
#endif
    }
}
