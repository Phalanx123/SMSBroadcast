using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmsBroadcast.Extensions.DependencyInjection;
using SmsBroadcast.Samples.Console;

var builder = Host.CreateApplicationBuilder(args);

// Configuration: appsettings.json + env overrides (prefix: SMSB_)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>(); // A first

builder.Configuration.AddEnvironmentVariables(prefix: "SMSB_");

// Logging: simple console
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.TimestampFormat = "HH:mm:ss ";
    o.SingleLine = true;
});

// DI: library + the sample runner
builder.Services.AddSmsBroadcast(builder.Configuration, "SmsBroadcast");
builder.Services.AddTransient<SampleRunner>();

using var host = builder.Build();

var runner = host.Services.GetRequiredService<SampleRunner>();
await runner.RunAsync();