using MQTTnet.Diagnostics;

namespace MQTTnet.Server
{
    public interface IMqttServerFactory
    {
        IMqttServer CreateMqttServer();

        IMqttServer CreateMqttServer(IMqttNetLogger logger);
    }
}