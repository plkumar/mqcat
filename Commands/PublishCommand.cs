using System.CommandLine;
using System.Text;
using mqcat.Bindings;
using mqcat.Util;
using RabbitMQ.Client;

namespace mqcat.Commands;

public class PublishCommand : Command
{
    private const int ErrorExitCode = -1;
    private readonly Option<string> _hostUriOption = new(aliases: new[]  {"--host", "--ho"}, description: "Host uri to connect.");
    
    private readonly Option<string> _serverOption = new(aliases: new[]  {"--server", "-S", "-s"}, description: "Server name to connect.");
    private readonly Option<string> _vhostOption = new(aliases: new[]  {"--vhost", "-V", "-v"}, description: "vhost to connect.");
    private readonly Option<string> _usernameOption = new(aliases: new[]  {"--user", "-u"}, description: "Host name to connect.");
    private readonly Option<string> _passwordOption = new(aliases: new[]  {"--password", "-p"}, description: "Host name to connect.");
    
    private readonly Option<string> _messageOption = new(aliases: new[]  {"--message", "-m"}, description: "Message to publish");
    private readonly Option<FileInfo> _fileOption = new(aliases: new[]  {"--file", "--FILE", "-f"}, description: "File path, contents of the file will be published.");
    private readonly Option<string> _exchangeOption = new(aliases: new[]  {"--exchange", "-e"}, description: "Message to publish");
    private readonly Option<string> _routingKeyOption = new(aliases: new[]  {"--routing-key", "-r"}, description: "Message to publish");
    private readonly Option<Boolean> _peerVerifyOption =
        new(aliases: new[] { "--disable-ssl-verify", "--dsv", "-d" }, "Disable SSL peer verify.");

    public PublishCommand() : base("publish", "Publishes messages to queue")
    {
        // hostOption.IsRequired=true;
        _hostUriOption.AddCompletions(new[] { "amqp://guest:guest@127.0.0.1:5672[/vhost]" });
        this.AddOption(_hostUriOption);

        _serverOption.AddCompletions(new[] { "localhost" });
        this.AddOption(_serverOption);
        this.AddOption(_vhostOption);
        this.AddOption(_usernameOption);
        this.AddOption(_passwordOption);
        
        _exchangeOption.IsRequired=true;
        this.AddOption(_exchangeOption);
        
        _routingKeyOption.IsRequired=true;
        this.AddOption(_routingKeyOption);

        _messageOption.IsRequired=false;
        this.AddOption(_messageOption);

        _fileOption.IsRequired = false;
        this.AddOption(_fileOption);

        _peerVerifyOption.IsRequired = false;
        _peerVerifyOption.SetDefaultValue(value: false);
        this.AddOption(_peerVerifyOption);
        
        this.SetHandler(Publish, _hostUriOption, new HostBinder(_serverOption, _vhostOption, _usernameOption, _passwordOption), _exchangeOption, _routingKeyOption, _messageOption, _fileOption, _peerVerifyOption);
    }

    void Publish(string hostUri, Host server, string exchangeName, string routingKey, string message, FileInfo? fileInfo, bool peerVerifyDisabled)
    {
        Utils.LogInfo($" {hostUri}, {exchangeName}, {routingKey}");
        
        var factory = new ConnectionFactory();

        // var sslSettings = new SslOption
        // {
        //     Enabled = true,
        //     ServerName = hostUri.DnsSafeHost,
        //     Version = SslProtocols.Tls12
        // };
        //
        
        if(peerVerifyDisabled)
        {
            factory.Ssl.CertificateValidationCallback += (sender, certificate, chain, errors) =>
            {
                return true;
            };
        }

        
        if (!string.IsNullOrEmpty(hostUri))
        {
            factory.Uri = new Uri(hostUri);    
        } else if (!string.IsNullOrEmpty(server.ServerName))
        {
            factory.HostName = server.ServerName;
        }
        else
        {
            Utils.LogError($"Error:: neither --host nor --server supplied, one of them is mandatory.");
        }

        if (!string.IsNullOrEmpty(server.UserName) && !string.IsNullOrEmpty(server.Password))
        {
            factory.UserName = server.UserName;
            factory.Password = server.Password;
        }

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