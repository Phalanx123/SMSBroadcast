using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneOf;
using SmsBroadcast.Abstractions;

namespace SmsBroadcast;

/// <inheritdoc />
public sealed class SmsBroadcastClient : ISmsBroadcastClient
{
    private readonly HttpClient _http;
    private readonly SmsBroadcastOptions _options;
    private readonly ILogger<SmsBroadcastClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmsBroadcastClient"/> class.
    /// </summary>
    /// <param name="http"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SmsBroadcastClient(
        HttpClient http,
        IOptions<SmsBroadcastOptions> options,
        ILogger<SmsBroadcastClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;

        _http.BaseAddress ??= _options.BaseUri;
        _http.Timeout = _options.Timeout;
    }

    /// <inheritdoc />
    public async Task<OneOf<SmsSendOutcome, SmsRequestError>> SendAsync(
        SendRequest request,
        CancellationToken ct = default)
    {
        var validation = Validate(request);
        if (validation is not null)
            return new SmsRequestError("Validation", validation);

        var form = BuildSendForm(request);
        using var content = new FormUrlEncodedContent(form);
        using var msg = new HttpRequestMessage(HttpMethod.Post, _options.EndpointPath);
        msg.Content = content;

        try
        {
            using var res = await _http.SendAsync(msg, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("HTTP {Status}. Body: {Body}", res.StatusCode, body);
                return new SmsRequestError(((int)res.StatusCode).ToString(), "HTTP error");
            }

            // Parse per-line outcomes
            var lines = body.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 1 && IsErrorLine(lines[0], out var reqErr))
                return reqErr!;

            var accepted = new List<SmsAccepted>();
            var rejected = new List<SmsRejected>();

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0) continue;

                if (IsErrorLine(line, out var requestLevelError))
                    return requestLevelError!;

                // Expected: "OK: 61400111222:2942263" OR "BAD:0400abc111:Invalid Number"
                var parts = line.Split(':', 3, StringSplitOptions.TrimEntries);
                if (parts.Length < 3)
                {
                    _logger.LogWarning("Unrecognised response line: {Line}", line);
                    rejected.Add(new SmsRejected(string.Empty, $"Unrecognised line: {line}"));
                    continue;
                }

                var status = parts[0]; // "OK" or "BAD"
                var number = parts[1]; // international or submitted format
                var trailing = parts[2]; // SmsRef (OK) OR Reason (BAD)

                if (status.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    accepted.Add(new SmsAccepted(number, trailing));
                }
                else if (status.Equals("BAD", StringComparison.OrdinalIgnoreCase))
                {
                    rejected.Add(new SmsRejected(number, trailing));
                }
                else
                {
                    // Defensive: treat unknown statuses as request-level error
                    return new SmsRequestError("ParseError", $"Unknown status '{status}' in line: {line}");
                }
            }

            return new SmsSendOutcome(accepted, rejected);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "SendAsync timeout");
            return new SmsRequestError("Timeout", "Request timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendAsync exception");
            return new SmsRequestError("Exception", ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<OneOf<int, SmsRequestError>> GetBalanceAsync(CancellationToken ct = default)
    {
        // GET api-adv.php?action=balance&username=...&password=...
        var url =
            $"{_options.EndpointPath}?action=balance&username={Uri.EscapeDataString(_options.Username)}&password={Uri.EscapeDataString(_options.Password)}";

        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        try
        {
            using var res = await _http.SendAsync(msg, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("HTTP {Status} on balance. Body: {Body}", res.StatusCode, body);
                return new SmsRequestError(((int)res.StatusCode).ToString(), "HTTP error");
            }

            // Expected: "OK: 5000" or "ERROR: message"
            var line = body.Trim();
            if (IsErrorLine(line, out var reqErr))
                return reqErr!;

            var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !parts[0].Equals("OK", StringComparison.OrdinalIgnoreCase))
                return new SmsRequestError("ParseError", $"Unexpected balance line: {line}");

            if (int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var credits))
                return credits;

            return new SmsRequestError("ParseError", $"Cannot parse balance: {parts[1]}");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "GetBalanceAsync timeout");
            return new SmsRequestError("Timeout", "Request timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetBalanceAsync exception");
            return new SmsRequestError("Exception", ex.Message);
        }
    }

    private Dictionary<string, string> BuildSendForm(SendRequest request)
    {
        var dict = new Dictionary<string, string>
        {
            ["username"] = _options.Username,
            ["password"] = _options.Password,
            ["to"] = string.Join(",", request.To),
            // FormUrlEncodedContent will correctly URL-encode text (including '\n' -> %0A).
            ["message"] = request.Message
        };

        if (!string.IsNullOrWhiteSpace(request.From)) dict["from"] = request.From!;
        if (!string.IsNullOrWhiteSpace(request.Reference)) dict["ref"] = request.Reference!;
        if (request.MaxSplit is { } ms) dict["maxsplit"] = ms.ToString(CultureInfo.InvariantCulture);
        if (request.DelayMinutes is { } d) dict["delay"] = d.ToString(CultureInfo.InvariantCulture);

        return dict;
    }

    private static string? Validate(SendRequest req)
    {
        if (req.To.Count == 0) return "At least one recipient is required.";
        if (string.IsNullOrWhiteSpace(req.Message)) return "Message is required.";
        if (req.MaxSplit is < 1 or > 5) return "maxsplit must be between 1 and 5.";
        return req.From is { Length: > 11 } ? "from must be 11 chars or fewer." : null;
    }
    
    private static bool IsErrorLine(string line, out SmsRequestError? err)
    {
        err = null;
        if (!line.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase)) return false;
        var msg = line["ERROR:".Length..].Trim();
        err = new SmsRequestError("ERROR", msg);
        return true;
    }
}