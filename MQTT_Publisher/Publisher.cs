using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MQTTPublisher
{
    internal class Publisher
    {
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();
            var client = mqttFactory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                           .WithClientId(Guid.NewGuid().ToString())
                            .WithTcpServer("test.mosquitto.org", 1883)
                            .WithCleanSession()
                            .Build();

            client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Broker bağlantısı başarılı !!");

                var topicFilter = new TopicFilterBuilder()
                                    .WithTopic("Node_Server")
                                    .Build();

                await client.SubscribeAsync(topicFilter);
            });

            client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Broker dan bağlantı kesildi");
            });

            client.UseApplicationMessageReceivedHandler(e =>    
            {
              
                Console.WriteLine($"Sub Gelen Mesaj: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            });

            await client.ConnectAsync(options);         

            Console.WriteLine("Lütfen pub edilecek mesajı yazın");
            do
            {
                string messagePayload = Console.ReadLine();
                if (string.IsNullOrEmpty(messagePayload))
                {
                    await client.DisconnectAsync(); 
                    break;
                }
                await PublishMessageAsync(client, messagePayload);
            } while (true);



        }
        private static async Task PublishMessageAsync(IMqttClient client, string messagePayload)
        {
            var message = new MqttApplicationMessageBuilder()  
                          .WithTopic("Node_Client")
                          .WithPayload(messagePayload)
                          .WithAtLeastOnceQoS()          
                        .Build();

            if (client.IsConnected)
            {
                await client.PublishAsync(message);

                Console.WriteLine($"Pub Giden Mesaj: {messagePayload}");

            }

        }
    }
}
