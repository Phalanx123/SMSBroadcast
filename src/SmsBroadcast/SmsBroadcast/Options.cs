using System.ComponentModel.DataAnnotations;

namespace SmsBroadcast;

public sealed class SmsBroadcastOptions
{
    [Required] public string Username { get; init; } = default!;
    [Required] public string Password { get; init; } = default!;

    /// <summary>Base API host; keep default per docs.</summary>
    public Uri BaseUri { get; init; } = new("https://api.smsbroadcast.com.au");

    /// <summary>Advanced API endpoint file name.</summary>
    public string EndpointPath { get; init; } = "/api-adv.php";

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}