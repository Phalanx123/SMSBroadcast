using System.ComponentModel.DataAnnotations;

namespace SmsBroadcast;

/// <summary>
/// Options for configuring the SMS Broadcast client.
/// </summary>
public sealed class SmsBroadcastOptions
{
    /// <summary>
    /// Username for an SMS Broadcast account.
    /// </summary>
    [Required] public string Username { get; set; } = null!;
    /// <summary>
    /// Password for an SMS Broadcast account.
    /// </summary>
    [Required] public string Password { get; set; } = null!;

    /// <summary>Base API host; keep default per docs.</summary>
    public Uri BaseUri { get; init; } = new("https://api.smsbroadcast.com.au");

    /// <summary>Advanced API endpoint file name.</summary>
    public string EndpointPath { get; init; } = "/api-adv.php";

    /// <summary>
    /// Timeout for HTTP requests to the SMS Broadcast API.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}