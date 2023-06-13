using System.CommandLine;
using System.Text;
using mqcat.Commands;
using RabbitMQ.Client;

namespace mqcat;

class Program
{
    static async Task<int> Main(string[] args)
    {

        var rootCommand = new RootCommand("mqcat - A commandline message queue client application.");
        
        rootCommand.AddCommand(new PublishCommand());
        rootCommand.AddCommand(new SubscribeCommand());

        return await rootCommand.InvokeAsync(args);
    }
}
