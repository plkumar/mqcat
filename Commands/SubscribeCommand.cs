using System.CommandLine;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace mqcat.Commands;

public class SubscribeCommand : Command
{
    private readonly Option<string> hostOption = new(aliases: new string[]  {"--host", "-h"}, description: "Host name to connect.");
    private readonly Option<string> queueOption = new(aliases: new string[]  {"--queue", "-q"}, description: "Queue name to publish message into.");
    private readonly Option<bool> isDurableOption = new(aliases: new string[] {"--durable", "-d"}, "Is queue durable.");
    private readonly Option<bool> waitOption = new(aliases: new string[] {"--wait", "-w"}, "shoud command wait for new messages.");

    public SubscribeCommand() : base("subscribe", "Subscribes to message queue.")
    {
        hostOption.IsRequired=true;
        this.AddOption(hostOption);
        queueOption.IsRequired=true;
        this.AddOption(queueOption);

        isDurableOption.SetDefaultValue(false);
        this.AddOption(isDurableOption);

        this.SetHandler<string, string, bool>((host, queueName, isDurable)=>{
            Subscribe(host, queueName, isDurable);
        }, hostOption, queueOption, isDurableOption);
    }

    private void Subscribe(string host, string queueName, bool isDurable)
    {
        var factory = new ConnectionFactory() { Uri = new Uri(host) };
        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName, durable: isDurable, exclusive: false, autoDelete: false, arguments: null);

                // Set up a consumer to receive messages
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Received message: {0}", message);
                };

                channel.QueueBind(queueName, "demo-exchange", "/temp/#");
                // Start consuming messages from the queue
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }
}