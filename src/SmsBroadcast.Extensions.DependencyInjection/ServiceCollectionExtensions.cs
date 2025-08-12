using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using SmsBroadcast.Abstractions;

namespace SmsBroadcast.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmsBroadcast(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "SmsBroadcast")
    {
        services.AddOptions<SmsBroadcastOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.Username) && !string.IsNullOrWhiteSpace(o.Password),
                "Username and Password are required.");

        services.AddHttpClient<ISmsBroadcastClient, SmsBroadcastClient>()
            .ConfigureHttpClient((sp, http) =>
            {
                var opts = sp.GetRequiredService<IOptions<SmsBroadcastOptions>>().Value;
                http.BaseAddress = opts.BaseUri;
                http.Timeout     = opts.Timeout;
            })
            .AddPolicyHandler(DefaultRetry());

        return services;
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