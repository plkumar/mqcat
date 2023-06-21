
using mqcat.Bindings;
using mqcat.Util;
using RabbitMQ.Client;

namespace mqcat.Client;

class AMQPClient
{
    public static ConnectionFactory GetConnectionFactory(string host) => new() { Uri = new Uri(host) };

    public static ConnectionFactory GetConnectionFactory(Host host)
    {
        var factory = new ConnectionFactory();
        if (!string.IsNullOrEmpty(host.ServerName))
        {
            factory.HostName = host.ServerName;
            factory.Port = host.Port;
        }

        if (!string.IsNullOrEmpty(host.UserName))
        {
            factory.UserName = host.UserName;
            if (string.IsNullOrEmpty(host.Password))
            {
                host.Password = Utils.ReadPassword();
            }
            factory.Password = host.Password;
        }

        return factory;
    }
}