using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using SmsBroadcast.Abstractions;

namespace SmsBroadcast.Extensions.DependencyInjection;

/// <summary>
///  Extension methods for IServiceCollection to register SmsBroadcast services.
/// </summary>
public static class ServiceCollectionExtensions
{
    
    /// <summary>
    /// Adds SmsBroadcast services to the service collection using default configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmsBroadcast(this IServiceCollection services)
        => services.AddSmsBroadcast("SmsBroadcast");
    /// <summary>
    ///  Adds SmsBroadcast services to the service collection using a configuration section path.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="sectionPath"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmsBroadcast(this IServiceCollection services, string sectionPath)
    {
        services.AddOptions<SmsBroadcastOptions>()
            .BindConfiguration(sectionPath) // <- pulls IConfiguration from the container
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.Username) &&
                           !string.IsNullOrWhiteSpace(o.Password),
                "Username and Password are required.");

        services.AddHttpClient<ISmsBroadcastClient, SmsBroadcastClient>()
            .ConfigureHttpClient((sp, http) =>
            {
                var opts = sp.GetRequiredService<IOptions<SmsBroadcastOptions>>().Value;
                http.BaseAddress = opts.BaseUri;
                http.Timeout     = opts.Timeout;
            })
            .AddPolicyHandler(DefaultRetry());

        services.TryAddSingleton<ISmsBroadcastClientFactory, SmsBroadcastClientFactory>();
        return services;
    }
    /// <summary>
    /// Adds SmsBroadcast services to the service collection using configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmsBroadcast(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "SmsBroadcast")
        => services.AddSmsBroadcast(configuration.GetSection(sectionName));


    /// <summary>
    /// Adds SmsBroadcast services to the service collection using a configuration section.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="section"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmsBroadcast(
        this IServiceCollection services,
        IConfigurationSection section)
        => services.AddSmsBroadcast(section.Bind);

    /// <summary>
    /// Adds SmsBroadcast services to the service collection using code-based options.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmsBroadcast(
        this IServiceCollection services,
        Action<SmsBroadcastOptions> configure)
    {
        services.AddOptions<SmsBroadcastOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.Username) &&
                           !string.IsNullOrWhiteSpace(o.Password),
                "Username and Password are required.");

        services.AddHttpClient<ISmsBroadcastClient, SmsBroadcastClient>()
            .ConfigureHttpClient((sp, http) =>
            {
                var opts = sp.GetRequiredService<IOptions<SmsBroadcastOptions>>().Value;
                http.BaseAddress = opts.BaseUri;
                http.Timeout     = opts.Timeout;
            })
            .AddPolicyHandler(DefaultRetry());

        // Ensure a single factory service exists
        services.TryAddSingleton<ISmsBroadcastClientFactory, SmsBroadcastClientFactory>();
        return services;
    }

    /// <summary>
    /// Adds SmsBroadcast services to the service collection using username and password.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmsBroadcast(
        this IServiceCollection services,
        string username,
        string password,
        Action<SmsBroadcastOptions>? configure = null)
        => services.AddSmsBroadcast(o =>
        {
            o.Username = username;
            o.Password = password;
            configure?.Invoke(o);
        });
    
    /// <summary>
    /// Adds a named SmsBroadcast client to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <param name="configureNamed"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddSmsBroadcast(
        this IServiceCollection services,
        string name,
        Action<SmsBroadcastOptions> configureNamed)
    {
        services.AddOptions<SmsBroadcastOptions>(name)
            .Configure(configureNamed)
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.Username) &&
                           !string.IsNullOrWhiteSpace(o.Password),
                "Username and Password are required.");

        var builder = services.AddHttpClient(name)
            .ConfigureHttpClient((sp, http) =>
            {
                var opts = sp.GetRequiredService<IOptionsMonitor<SmsBroadcastOptions>>().Get(name);
                http.BaseAddress = opts.BaseUri;
                http.Timeout     = opts.Timeout;
            })
            .AddPolicyHandler(DefaultRetry());

        // Ensure factory is available
        services.TryAddSingleton<ISmsBroadcastClientFactory, SmsBroadcastClientFactory>();
        return builder;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> DefaultRetry() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync([
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5)
            ]);
}
