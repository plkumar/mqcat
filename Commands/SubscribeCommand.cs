using System.CommandLine;
using System.Text;
using mqcat.Bindings;
using mqcat.Client;
using mqcat.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace mqcat.Commands;

public sealed class SubscribeCommand : Command
{
    private readonly Option<string> _hostUriOption = new(aliases: new[] { "--host", "--ho" }, description: "Host name to connect.");
    
    private readonly Option<string> _serverOption = new(aliases: new[] { "--server", "-S", "-s" }, description: "Server name to connect.");
    private readonly Option<int> _portOption = new(aliases: new[] { "--port", "-P" }, description: "Port number to connect.");
    private readonly Option<string> _vhostOption = new(aliases: new[] { "--vhost", "-V", "-v" }, description: "vhost to connect.");
    private readonly Option<string> _usernameOption = new(aliases: new[] { "--user", "-u" }, description: "Host name to connect.");
    private readonly Option<string> _passwordOption = new(aliases: new[] { "--password", "-p" }, description: "Host name to connect.");

    private readonly Option<string> _queueOption = new(aliases: new[] { "--queue", "-q" }, description: "Queue name to publish message into.");
    private readonly Option<bool> _isDurableOption = new(aliases: new[] { "--durable", "-d" }, "Is queue durable.");
    private readonly Option<bool> _waitOption = new(aliases: new[] { "--wait", "-w" }, "should command wait for new messages.");

    private bool _keepRunning = true;

    public SubscribeCommand() : base("subscribe", "Subscribes to message queue.")
    {
        _hostUriOption.IsRequired = true;
        this.AddOption(_hostUriOption);

        _serverOption.AddCompletions(new[] { "localhost" });
        this.AddOption(_serverOption);
        _portOption.SetDefaultValue(value: 5672);
        this.AddOption(_portOption);
        _vhostOption.AddCompletions(new[] { "/", "/vhost1" });
        _vhostOption.SetDefaultValue(value: "/");
        this.AddOption(_vhostOption);
        this.AddOption(_usernameOption);
        this.AddOption(_passwordOption);

        _queueOption.IsRequired = true;
        this.AddOption(_queueOption);

        _isDurableOption.SetDefaultValue(value: false);
        this.AddOption(_isDurableOption);

        _waitOption.SetDefaultValue(value: false);
        this.AddOption(_waitOption);

        this.SetHandler(handle: Subscribe, _hostUriOption, new HostBinder(_serverOption, _portOption, _vhostOption, _usernameOption, _passwordOption), _queueOption, _isDurableOption, _waitOption);
    }

    private Task<int> Subscribe(string hostUri, Host host, string queueName, bool isDurable, bool shouldWait)
    {
        ConnectionFactory? factory = null;

        if (string.IsNullOrEmpty(hostUri))
        {
            if (string.IsNullOrEmpty(host.ServerName))
            {
                Utils.LogError("neither --host nor --server supplied, one of them is mandatory.");
                Task.FromResult(-1);
            }
            else
            {
                factory = AMQPClient.GetConnectionFactory(host);
            }
        }
        else
        {
            factory = AMQPClient.GetConnectionFactory(hostUri);
        }

        using (var connection = factory.CreateConnection())
        {
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName, durable: isDurable, exclusive: false, autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (_, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Received message: {0}", message);
            };

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Closing Connection.");
                e.Cancel = true;
                this._keepRunning = false;
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            if (shouldWait)
            {
                Console.WriteLine("Waiting for new messages, Press Ctrl+C to exit...");
                while (_keepRunning && channel.IsOpen) Thread.Sleep(2);
            }
            else
            {
                // wait for sometime before closing to complete reading messages.
                Thread.Sleep(500);
            }
            connection.Close();
            return Task.FromResult(0);
        }
    }
}