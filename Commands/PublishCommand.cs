using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Text;
using RabbitMQ.Client;

namespace mqcat.Commands;

public class PublishCommand : Command
{
    private readonly Option<string> hostOption = new(aliases: new string[]  {"--host", "-h"}, description: "Host name to connect.");
    private readonly Option<string> messageOption = new(aliases: new string[]  {"--message", "-m"}, description: "Message to publish");
    private readonly Option<string> exchangeOption = new(aliases: new string[]  {"--exchange", "-e"}, description: "Message to publish");
    private readonly Option<string> routingKeyOption = new(aliases: new string[]  {"--routing-key", "-r"}, description: "Message to publish");

    public PublishCommand() : base("publish", "Publishes messages to queue")
    {
        hostOption.IsRequired=true;
        this.AddOption(hostOption);
        
        exchangeOption.IsRequired=true;
        this.AddOption(exchangeOption);
        
        routingKeyOption.IsRequired=true;
        this.AddOption(routingKeyOption);

        messageOption.IsRequired=true;
        this.AddOption(messageOption);
       
        this.SetHandler<string, string, string, string>((host, exchange, routingKey,message)=>{
            Publish(host,exchange, routingKey, message);
        }, hostOption, exchangeOption, routingKeyOption, messageOption);
    }

    void Publish(string host, string exchangeName, string routingKey, string message)
    {
        var factory = new ConnectionFactory() { HostName = host };
        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                // Declare the exchange                
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

                // Publish a message
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