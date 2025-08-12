using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmsBroadcast.Extensions.DependencyInjection;
using SmsBroadcast.Samples.Console;

var builder = Host.CreateApplicationBuilder(args);

// Order: json -> user-secrets -> env vars (env wins)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddUserSecrets<Program>()                       // ← move out of the Development check
    .AddEnvironmentVariables(prefix: "SMSB_");       // ← the highest precedence

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.TimestampFormat = "HH:mm:ss ";
    o.SingleLine = true;
});

builder.Services.AddSmsBroadcast(builder.Configuration);
builder.Services.AddTransient<SampleRunner>();

using var host = builder.Build();
await host.Services.GetRequiredService<SampleRunner>().RunAsync();