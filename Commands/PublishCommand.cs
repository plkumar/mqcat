using System.CommandLine;
using System.Text;
using mqcat.Util;
using RabbitMQ.Client;

namespace mqcat.Commands;

public class PublishCommand : Command
{
    private const int ErrorExitCode = -1;
    private readonly Option<string> hostOption = new(aliases: new[]  {"--host", "--ho"}, description: "Host uri to connect.");
    
    private readonly Option<string> serverOption = new(aliases: new[]  {"--server", "-S", "-s"}, description: "Server name to connect.");
    private readonly Option<string> usernameOption = new(aliases: new[]  {"--user", "-u"}, description: "Host name to connect.");
    private readonly Option<string> passwordOption = new(aliases: new[]  {"--password", "-p"}, description: "Host name to connect.");
    
    private readonly Option<string> messageOption = new(aliases: new[]  {"--message", "-m"}, description: "Message to publish");
    private readonly Option<FileInfo> fileOption = new(aliases: new[]  {"--file", "--FILE", "-f"}, description: "File path, contents of the file will be published.");
    private readonly Option<string> exchangeOption = new(aliases: new[]  {"--exchange", "-e"}, description: "Message to publish");
    private readonly Option<string> routingKeyOption = new(aliases: new[]  {"--routing-key", "-r"}, description: "Message to publish");
    private readonly Option<Boolean> peerVerifyOption =
        new(aliases: new[] { "--disable-ssl-verify", "--dsv", "-d" }, "Disable SSL peer verify.");

    public PublishCommand() : base("publish", "Publishes messages to queue")
    {
        // hostOption.IsRequired=true;
        hostOption.AddCompletions(new[] { "amqp://guest:guest@127.0.0.1:5672[/vhost]" });
        this.AddOption(hostOption);

        serverOption.AddCompletions(new[] { "localhost" });
        this.AddOption(serverOption);
        this.AddOption(usernameOption);
        this.AddOption(passwordOption);
        
        exchangeOption.IsRequired=true;
        this.AddOption(exchangeOption);
        
        routingKeyOption.IsRequired=true;
        this.AddOption(routingKeyOption);

        messageOption.IsRequired=false;
        this.AddOption(messageOption);

        fileOption.IsRequired = false;
        this.AddOption(fileOption);

        peerVerifyOption.IsRequired = false;
        peerVerifyOption.SetDefaultValue(false);
        this.AddOption(peerVerifyOption);
        
        this.SetHandler(Publish, hostOption, exchangeOption, routingKeyOption, messageOption, fileOption, peerVerifyOption);
    }

    void Publish(string host, string exchangeName, string routingKey, string message, FileInfo? fileInfo, bool peerVerifyDisabled)
    {
        Utils.LogInfo($" {host}, {exchangeName}, {routingKey}");
        var hostUri = new Uri(host);
        // var sslSettings = new SslOption
        // {
        //     Enabled = true,
        //     ServerName = hostUri.DnsSafeHost,
        //     Version = SslProtocols.Tls12
        // };
        //
        // if(peerVerifyDisabled)
        // {
        //     sslSettings.CertificateValidationCallback += (sender, certificate, chain, errors) =>
        //     {
        //         return true;
        //     };
        // }
        
        var factory = new ConnectionFactory() { Uri=hostUri };

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