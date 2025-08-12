using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmsBroadcast.Abstractions;

namespace SmsBroadcast.Samples.Console;

public sealed class SampleRunner
{
    private readonly ISmsBroadcastClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<SampleRunner> _logger;

    public SampleRunner(
        ISmsBroadcastClient client,
        IConfiguration config,
        ILogger<SampleRunner> logger)
    {
        _client = client;
        _config = config;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        // Load sample settings
        var toSingle = _config["Sample:ToSingle"]; // e.g., 61400111222 (international format strongly recommended)
        var toBulk   = _config.GetSection("Sample:ToBulk").Get<string[]>() ?? [];
        var from     = _config["Sample:From"];     // <= 11 chars; optional
        if (string.IsNullOrWhiteSpace(toSingle))
        {
            _logger.LogWarning("No 'Sample:ToSingle' configured. Edit appsettings.json.");
            return;
        }

        // 1) Single send (demonstrates newline handling and ref/max-split)
        const string singleMessage = "test: line 1\nline 2 (new line)\nline 3\n— end —";

        var singleReq = new SendRequest(
            To: [toSingle],
            Message: singleMessage,
            From: from,
            Reference: "SampleRef-001",
            MaxSplit: 3,
            DelayMinutes: null
        );

        _logger.LogInformation("Sending single SMS to {To} ...", toSingle);
        var singleResult = await _client.SendAsync(singleReq);
        singleResult.Switch(
            ok =>
            {
                foreach (var a in ok.Accepted)
                    _logger.LogInformation("OK  -> {To} SmsRef={Ref}", a.To, a.SmsRef);
                foreach (var r in ok.Rejected)
                    _logger.LogWarning("BAD -> {To} Reason={Reason}", r.To, r.Reason);
            },
            err =>
            {
                _logger.LogError("ERROR -> {Code}: {Message}", err.Code, err.Message);
            });

        // 2) Bulk send (if any numbers configured)
        if (toBulk.Length > 0)
        {
            var bulkReq = new SendRequest(
                To: toBulk,
                Message: "Structura bulk test\nThis is a multi-recipient message.",
                From: from,
                Reference: "BulkRef-ABC",
                MaxSplit: 2,
                DelayMinutes: 1 // delay by 1 minute as an example
            );

            _logger.LogInformation("Sending bulk SMS to {Count} recipients ...", toBulk.Length);
            var bulkResult = await _client.SendAsync(bulkReq);
            bulkResult.Switch(
                ok =>
                {
                    foreach (var a in ok.Accepted)
                        _logger.LogInformation("OK  -> {To} SmsRef={Ref}", a.To, a.SmsRef);
                    foreach (var r in ok.Rejected)
                        _logger.LogWarning("BAD -> {To} Reason={Reason}", r.To, r.Reason);
                },
                err =>
                {
                    _logger.LogError("ERROR -> {Code}: {Message}", err.Code, err.Message);
                });
        }

        // 3) Balance check
        var bal = await _client.GetBalanceAsync();
        bal.Switch(
            credits => _logger.LogInformation("Balance: {Credits} credits", credits),
            err      => _logger.LogError("Balance ERROR -> {Code}: {Message}", err.Code, err.Message)
        );
    }
}