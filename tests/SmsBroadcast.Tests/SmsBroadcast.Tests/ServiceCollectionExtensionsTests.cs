using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmsBroadcast.Abstractions;
using SmsBroadcast.Extensions.DependencyInjection;

namespace SmsBroadcast.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSmsBroadcast_Throws_When_Credentials_Missing()
    {
        var services = new ServiceCollection();

        // Empty config -> Username/Password missing
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Intentionally not setting SmsBroadcast:Username/Password
            ["SmsBroadcast:BaseUri"] = "https://api.smsbroadcast.com.au",
            ["SmsBroadcast:EndpointPath"] = "/api-adv.php"
        }).Build();

        services.AddLogging();
        services.AddSmsBroadcast(cfg);

        using var sp = services.BuildServiceProvider();

        // Resolving the client forces options evaluation in ConfigureHttpClient -> validation triggers
        Assert.Throws<OptionsValidationException>(() => sp.GetRequiredService<ISmsBroadcastClient>());
    }

    [Fact]
    public void AddSmsBroadcast_Succeeds_With_Credentials()
    {
        var services = new ServiceCollection();

        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SmsBroadcast:Username"] = "u",
            ["SmsBroadcast:Password"] = "p",
            ["SmsBroadcast:BaseUri"] = "https://api.smsbroadcast.com.au",
            ["SmsBroadcast:EndpointPath"] = "/api-adv.php"
        }).Build();

        services.AddLogging();
        services.AddSmsBroadcast(cfg);

        using var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<ISmsBroadcastClient>();
        Assert.NotNull(client);
    }
}