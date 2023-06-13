using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Text;
using RabbitMQ.Client;

namespace mqcat.Commands;

public class PublishCommand : Command
{
    private readonly Option<string> messageOption = new(aliases: new string[]  {"--message", "-m"}, description: "Message to publish");

    public PublishCommand() : base("publish", "Publishes messages to queue")
    {
        messageOption.IsRequired=true;
        this.AddOption(messageOption);
        this.SetHandler<string>((message)=>{
            Publish(message);
        }, messageOption);
    }

    void Publish(string message)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                // Declare the exchange
                string exchangeName = "my_exchange";
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

                // Publish a message
                string routingKey = "my_routing_key";
                //string message = "Hello, RabbitMQ!";
                byte[] body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: exchangeName,
                                     routingKey: routingKey,
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine("Message sent: {0}", message);
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }
}