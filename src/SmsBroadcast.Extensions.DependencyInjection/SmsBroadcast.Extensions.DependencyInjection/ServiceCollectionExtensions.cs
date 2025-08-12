using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using SmsBroadcast.Abstractions;

namespace SmsBroadcast.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmsBroadcast(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "SmsBroadcast")
    {
        services.AddOptions<SmsBroadcast.SmsBroadcastOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.Username) && !string.IsNullOrWhiteSpace(o.Password),
                "Username and Password are required.");

        services.AddHttpClient<ISmsBroadcastClient, SmsBroadcast.SmsBroadcastClient>()
            .ConfigureHttpClient((sp, http) =>
            {
                var opts = sp.GetRequiredService<IOptions<SmsBroadcast.SmsBroadcastOptions>>().Value;
                http.BaseAddress = opts.BaseUri;
                http.Timeout     = opts.Timeout;
            })
            .AddPolicyHandler(DefaultRetry());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> DefaultRetry() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5)
            });
}