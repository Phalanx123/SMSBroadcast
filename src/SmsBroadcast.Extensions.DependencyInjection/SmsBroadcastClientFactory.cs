using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmsBroadcast.Abstractions;

namespace SmsBroadcast.Extensions.DependencyInjection;

/// <summary>
/// Creates ISmsBroadcastClient instances from the default or a named HttpClient registration.
/// </summary>
internal sealed class SmsBroadcastClientFactory : ISmsBroadcastClientFactory
{
    private readonly IServiceProvider _root;
    private readonly IHttpClientFactory _http;
    private readonly IOptionsMonitor<SmsBroadcastOptions> _options;
    private readonly ILoggerFactory _logFactory;

    public SmsBroadcastClientFactory(
        IServiceProvider root,
        IHttpClientFactory http,
        IOptionsMonitor<SmsBroadcastOptions> options,
        ILoggerFactory logFactory)
    {
        _root = root;
        _http = http;
        _options = options;
        _logFactory = logFactory;
    }

    public ISmsBroadcastClient GetClient()
    {
        // Use the typed default client already registered in DI.
        return _root.GetRequiredService<ISmsBroadcastClient>();
    }

    public ISmsBroadcastClient GetClient(string name)
    {
        // Build an ad-hoc client bound to the named registration.
        var http = _http.CreateClient(name);
        var opts = _options.Get(name);
        var logger = _logFactory.CreateLogger<SmsBroadcastClient>();
        return new SmsBroadcastClient(http, Options.Create(opts), logger);
    }
}