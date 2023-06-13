using System.CommandLine;
using System.Text;
using mqcat.Util;
using RabbitMQ.Client;

namespace mqcat.Commands;

public class PublishCommand : Command
{
    private const int ErrorExitCode = -1;
    private readonly Option<string> hostOption = new(aliases: new[]  {"--host", "-h"}, description: "Host name to connect.");
    private readonly Option<string> messageOption = new(aliases: new[]  {"--message", "-m"}, description: "Message to publish");
    private readonly Option<FileInfo> fileOption = new(aliases: new[]  {"--file", "--FILE", "-f"}, description: "File path, contents of the file will be published.");
    private readonly Option<string> exchangeOption = new(aliases: new[]  {"--exchange", "-e"}, description: "Message to publish");
    private readonly Option<string> routingKeyOption = new(aliases: new[]  {"--routing-key", "-r"}, description: "Message to publish");

    public PublishCommand() : base("publish", "Publishes messages to queue")
    {
        hostOption.IsRequired=true;
        hostOption.AddCompletions(new[] { "amqp://guest:guest@127.0.0.1:5672" });
        this.AddOption(hostOption);
        
        exchangeOption.IsRequired=true;
        this.AddOption(exchangeOption);
        
        routingKeyOption.IsRequired=true;
        this.AddOption(routingKeyOption);

        messageOption.IsRequired=false;
        this.AddOption(messageOption);

        fileOption.IsRequired = false;
        this.AddOption(fileOption);
        
        this.SetHandler(Publish, hostOption, exchangeOption, routingKeyOption, messageOption, fileOption);
    }

    void Publish(string host, string exchangeName, string routingKey, string message, FileInfo? fileInfo)
    {
        Utils.LogInfo($" {host}, {exchangeName}, {routingKey}");

        var factory = new ConnectionFactory() { Uri=new Uri(host) };
        using var connection = factory.CreateConnection();
        
        if(string.IsNullOrEmpty(message) && fileInfo==null && !Console.IsInputRedirected)
        {
            Utils.LogError("ERROR :: Neither message nor file argument specified and no input redirection detected.");
            Environment.Exit(ErrorExitCode);
        }
        
        if (string.IsNullOrEmpty(message) && Console.IsInputRedirected)
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(8192))); // This will allow input >256 chars
            if (Console.In.Peek() != -1)
            {
                message = Console.In.ReadToEnd();
            }
        } else if (fileInfo != null)
        {
            message = File.ReadAllText(fileInfo.FullName);
        }

        using var channel = connection.CreateModel();
        // Declare the exchange
        channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, true);

        // Publish a message
        byte[] body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        Utils.LogSuccess($"Message sent:\n {message}" );
    }
}