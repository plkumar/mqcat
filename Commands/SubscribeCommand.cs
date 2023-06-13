using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace mqcat.Commands;

public class SubscribeCommand : Command
{
    private readonly Option<string> hostOption = new(aliases: new string[]  {"--host", "-h"}, description: "Host name to connect.");
    private readonly Option<string> queueOption = new(aliases: new string[]  {"--queue", "-q"}, description: "Queue name to publish message into.");

    public SubscribeCommand() : base("subscribe", "Subscribes to message queue.")
    {
        hostOption.IsRequired=true;
        this.AddOption(hostOption);
        queueOption.IsRequired=true;
        this.AddOption(queueOption);
        this.SetHandler<string, string>((host, queueName)=>{
            Subscribe(host, queueName);
        }, hostOption, queueOption);
    }

    private void Subscribe(string host, string queueName)
    {
        var factory = new ConnectionFactory() { HostName = host };
        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                // Set up a consumer to receive messages
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Received message: {0}", message);
                };

                // Start consuming messages from the queue
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }
}