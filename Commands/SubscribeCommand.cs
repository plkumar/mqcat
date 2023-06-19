using System.CommandLine;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace mqcat.Commands;

public class SubscribeCommand : Command
{
    private readonly Option<string> _hostOption = new(aliases: new[]  {"--host", "--ho"}, description: "Host name to connect.");
    private readonly Option<string> _queueOption = new(aliases: new[]  {"--queue", "-q"}, description: "Queue name to publish message into.");
    private readonly Option<bool> _isDurableOption = new(aliases: new[] {"--durable", "-d"}, "Is queue durable.");
    private readonly Option<bool> _waitOption = new(aliases: new[] {"--wait", "-w"}, "should command wait for new messages.");
    
    private bool _keepRunning=true;
    
    public SubscribeCommand() : base("subscribe", "Subscribes to message queue.")
    {
        _hostOption.IsRequired=true;
        this.AddOption(_hostOption);
        _queueOption.IsRequired=true;
        this.AddOption(_queueOption);

        _isDurableOption.SetDefaultValue(value: false);
        this.AddOption(_isDurableOption);

        _waitOption.SetDefaultValue(value: false);
        this.AddOption(_waitOption);
        
        this.SetHandler(handle: Subscribe, _hostOption, _queueOption, _isDurableOption, _waitOption);
    }

    private void Subscribe(string host, string queueName, bool isDurable, bool shouldWait)
    {
        var factory = new ConnectionFactory() { Uri = new Uri(host) };
        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName, durable: isDurable, exclusive: false, autoDelete: false,
                    arguments: null);

                // Set up a consumer to receive messages
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

                // channel.QueueBind(queueName, "demo-exchange", "/temp/#");
                // Start consuming messages from the queue
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
            }
        }
    }
}