using System.CommandLine;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;

namespace mqcat.Commands;

public class PublishCommand : Command
{
    private const int ErrorExitCode = -1;
    private readonly Option<string> hostOption = new(aliases: new[]  {"--host", "-h"}, description: "Host name to connect.");
    private readonly Option<string> messageOption = new(aliases: new[]  {"--message", "-m"}, description: "Message to publish");
    private readonly Option<string> exchangeOption = new(aliases: new[]  {"--exchange", "-e"}, description: "Message to publish");
    private readonly Option<string> routingKeyOption = new(aliases: new[]  {"--routing-key", "-r"}, description: "Message to publish");

    public PublishCommand() : base("publish", "Publishes messages to queue")
    {
        hostOption.IsRequired=true;
        this.AddOption(hostOption);
        
        exchangeOption.IsRequired=true;
        this.AddOption(exchangeOption);
        
        routingKeyOption.IsRequired=true;
        this.AddOption(routingKeyOption);

        messageOption.IsRequired=false;
        this.AddOption(messageOption);
       
        this.SetHandler(Publish, hostOption, exchangeOption, routingKeyOption, messageOption);
    }

    void Publish(string host, string exchangeName, string routingKey, string message)
    {
        Console.WriteLine($" {host}, {exchangeName}, {routingKey}");

        var factory = new ConnectionFactory() { Uri=new Uri(host) };
        using var connection = factory.CreateConnection();
        if (string.IsNullOrEmpty(message) && Console.IsInputRedirected)
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192))); // This will allow input >256 chars
            while (Console.In.Peek() != -1)
            {
                message+= Console.In.ReadLine();
            }
        } else {
            Console.Error.WriteLine("No Message passes or input redirection detected.");
            Environment.Exit(ErrorExitCode);
        }

        Console.WriteLine("Message : \n " + message + "\n");

        using var channel = connection.CreateModel();
        // Declare the exchange                
        channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, true);

        // Publish a message
        byte[] body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        //Console.WriteLine("Message sent: {0}", message);
    }
}