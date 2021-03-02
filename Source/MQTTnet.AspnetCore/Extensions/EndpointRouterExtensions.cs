
#if NETCOREAPP3_1 || NET5_0

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;

namespace MQTTnet.AspNetCore.Extensions
{
    public static class EndpointRouterExtensions
    {
        public static void MapMqtt(this IEndpointRouteBuilder endpoints, string pattern, Action<HttpConnectionDispatcherOptions> configureOptions = null) 
        {
            endpoints.MapConnectionHandler<MqttConnectionHandler>(pattern, options =>
            {
                options.WebSockets.SubProtocolSelector = MqttSubProtocolSelector.SelectSubProtocol;
                
                configureOptions?.Invoke(options);
            });
        }
    }
}

#endif
