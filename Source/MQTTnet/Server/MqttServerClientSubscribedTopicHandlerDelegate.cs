using System;
using System.Threading.Tasks;
using MQTTnet.Implementations;

namespace MQTTnet.Server
{
    public sealed class MqttServerClientSubscribedHandlerDelegate : IMqttServerClientSubscribedTopicHandler
    {
        readonly Func<MqttServerClientSubscribedTopicEventArgs, Task> _handler;

        public MqttServerClientSubscribedHandlerDelegate(Action<MqttServerClientSubscribedTopicEventArgs> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _handler = eventArgs =>
            {
                handler(eventArgs);
                return PlatformAbstractionLayer.CompletedTask;
            };
        }

        public MqttServerClientSubscribedHandlerDelegate(Func<MqttServerClientSubscribedTopicEventArgs, Task> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task HandleClientSubscribedTopicAsync(MqttServerClientSubscribedTopicEventArgs eventArgs)
        {
            return _handler(eventArgs);
        }
    }
}
