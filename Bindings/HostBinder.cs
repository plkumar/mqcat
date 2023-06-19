using System.CommandLine;
using System.CommandLine.Binding;

namespace mqcat.Bindings;

public class HostBinder : BinderBase<Host>
{
    private readonly Option<string> _serverOption;
    private readonly Option<string> _vhostOption;
    private readonly Option<string> _usernameOption;
    private readonly Option<string> _passwordOption;

    public HostBinder(Option<string> server, Option<string> vhost, Option<string> username, Option<string> password)
    {
        this._serverOption = server;
        this._vhostOption = vhost;
        this._usernameOption = username;
        this._passwordOption = password;
    }
    
    protected override Host GetBoundValue(BindingContext bindingContext)
    {
        return new Host
        {
            ServerName = bindingContext.ParseResult.GetValueForOption(_serverOption),
            Vhost = bindingContext.ParseResult.GetValueForOption(_vhostOption),
            UserName = bindingContext.ParseResult.GetValueForOption(_usernameOption),
            Password = bindingContext.ParseResult.GetValueForOption(_passwordOption)
        };
    }
}