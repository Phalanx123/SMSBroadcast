using OneOf;

namespace SmsBroadcast.Abstractions;

/// <summary>Request to send the same message to one or more recipients.</summary>
public sealed record SendRequest(
    IReadOnlyList<string> To,
    string Message,
    string? From = null,
    string? Reference = null,
    int? MaxSplit = null,
    int? DelayMinutes = null
);

/// <summary>One accepted SMS to a single recipient.</summary>
public sealed record SmsAccepted(string To, string SmsRef);

/// <summary>One rejected SMS (per-recipient validation failure).</summary>
public sealed record SmsRejected(string To, string Reason);

/// <summary>All per-recipient outcomes from a request that reached the API.</summary>
public sealed record SmsSendOutcome(
    IReadOnlyList<SmsAccepted> Accepted,
    IReadOnlyList<SmsRejected> Rejected
);

/// <summary>Request-level error (credentials missing/invalid, required param missing, etc.).</summary>
public sealed record SmsRequestError(string Code, string Message);

public interface ISmsBroadcastClient
{
    /// <summary>
    /// Sends an SMS (same message to one or many recipients).
    /// Returns per-recipient results when the request is accepted by the API,
    /// or a request-level error if the API returned a single ERROR line.
    /// </summary>
    Task<OneOf<SmsSendOutcome, SmsRequestError>> SendAsync(
        SendRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Returns account balance (credits). On error, returns SmsRequestError.
    /// </summary>
    Task<OneOf<int, SmsRequestError>> GetBalanceAsync(CancellationToken ct = default);
}