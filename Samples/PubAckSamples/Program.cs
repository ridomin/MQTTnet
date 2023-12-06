// See https://aka.ms/new-console-template for more information
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Formatter;
using MQTTnet.Packets;
using System.Net.WebSockets;
namespace PubAckSamples;
internal class Program
{

    static Guid rid = Guid.Empty;

    static Func<Guid, bool> _dropPackageCallback = pid =>
    {
        return pid == rid; 
    };

    private static async Task Main(string[] args)
    {
        var logger = new MqttNetEventLogger();
        MqttNetConsoleLogger.ForwardToConsole(logger);

        IMqttClient mqttClient = new MqttClient(new MyChannelFactory(_dropPackageCallback), logger);
        var ops = new MqttClientOptionsBuilder()
            .WithTcpServer("e4k.rido.dev", 8883)
            .WithTlsOptions(c => c.UseTls())
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(55))
            .WithCleanStart(false)
            .WithClientId("rpcTester2")
            .WithSessionExpiryInterval(uint.MaxValue)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .Build();

     
        var conAck = await mqttClient.ConnectAsync(ops);
        await Console.Out.WriteLineAsync("Connected");

        
        mqttClient.ApplicationMessageReceivedAsync += async m =>
        {
            await Console.Out.WriteLineAsync($"{m.PacketIdentifier} {m.ApplicationMessage.Topic}" );
        };

        await mqttClient.SubscribeAsync("test/rpc", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

        Guid myGuid = Guid.NewGuid();
        rid = myGuid;

        var puback = await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic("test/rpc")
            .WithPayload("inspecing")
            .WithCorrelationData(myGuid.ToByteArray())
            .Build());
        await Console.Out.WriteLineAsync($"pub packet: {puback.PacketIdentifier}");

        Console.ReadLine();
    }

    //mqttClient.InspectPacketAsync += m =>
    //{

    //    MqttBufferWriter bw = new MqttBufferWriter(4096, 65535);
    //    //bw.WriteBinary(m.Buffer);
    //    var packetFormatter = MqttPacketFormatterAdapter.GetMqttPacketFormatter(MqttProtocolVersion.V500, bw);

    //    if (m.Buffer.Length > 2)
    //    {
    //        var rp = new ReceivedMqttPacket(m.Buffer.First(), new ArraySegment<byte>(m.Buffer, 0, m.Buffer.Length), m.Buffer.Length + 2);

    //        byte fifty = 0x32;
    //        if (rp.FixedHeader == fifty)
    //        {
    //            var packet = packetFormatter.Decode(rp);
    //            Console.WriteLine($"Packet received. {m.Direction} {packet.GetRfcName()} ");
    //        }

    //    }
    //    return Task.CompletedTask;
    //};
}