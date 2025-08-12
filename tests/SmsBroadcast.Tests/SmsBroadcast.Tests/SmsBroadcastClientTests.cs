using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmsBroadcast.Abstractions;

namespace SmsBroadcast.Tests;

// Reusable stub so tests can control responses and inspect the outgoing request content.
internal sealed class CapturingHandler : HttpMessageHandler
{
    public string? LastRequestBody { get; private set; }

    public Func<HttpRequestMessage, HttpResponseMessage>? OnSend { get; set; }
    public Func<HttpRequestMessage, Exception>? OnThrow { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Capture the content BEFORE the client disposes of it
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        if (OnThrow is not null)
            throw OnThrow(request);

        return OnSend is not null
            ? OnSend(request)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) };
    }
}

public sealed class SmsBroadcastClientTests
{
    private static SmsBroadcastClient CreateClient(CapturingHandler handler, SmsBroadcastOptions? opts = null)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.smsbroadcast.com.au")
        };

        opts ??= new SmsBroadcastOptions
        {
            Username = "u",
            Password = "p",
            BaseUri  = new Uri("https://api.smsbroadcast.com.au"),
            EndpointPath = "/api-adv.php"
        };

        return new SmsBroadcastClient(http, Options.Create(opts), NullLogger<SmsBroadcastClient>.Instance);
    }

    [Fact]
    public async Task SendAsync_Parses_Ok_And_Bad_Lines()
    {
        var handler = new CapturingHandler
        {
            OnSend = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("OK: 61400111222:2942263\nBAD:0400abc111:Invalid Number\n", Encoding.UTF8)
            }
        };

        var client = CreateClient(handler);
        var res = await client.SendAsync(new SendRequest(["0400111222", "0400abc111"], "Hello"));

        Assert.True(res.IsT0, "Expected per-recipient outcome");
        var outcome = res.AsT0;
        Assert.Single(outcome.Accepted);
        Assert.Single(outcome.Rejected);
        Assert.Equal("61400111222", outcome.Accepted[0].To);
        Assert.Equal("2942263", outcome.Accepted[0].SmsRef);
        Assert.Equal("0400abc111", outcome.Rejected[0].To);
        Assert.Equal("Invalid Number", outcome.Rejected[0].Reason);
    }

    [Fact]
    public async Task SendAsync_RequestLevelError_Maps_To_SmsRequestError()
    {
        var handler = new CapturingHandler
        {
            OnSend = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ERROR: Username or password is incorrect\n", Encoding.UTF8)
            }
        };

        var client = CreateClient(handler);
        var res = await client.SendAsync(new SendRequest(["0400111222"], "Hello"));
        Assert.True(res.IsT1);
        Assert.Equal("ERROR", res.AsT1.Code);
    }

    [Fact]
    public async Task SendAsync_UnknownStatus_Returns_ParseError()
    {
        var handler = new CapturingHandler
        {
            OnSend = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("WUT: 61400111222:???\n", Encoding.UTF8)
            }
        };

        var client = CreateClient(handler);
        var res = await client.SendAsync(new SendRequest(["0400111222"], "Hello"));
        Assert.True(res.IsT1);
        Assert.Equal("ParseError", res.AsT1.Code);
    }

    [Fact]
    public async Task SendAsync_Non2xx_Returns_HttpError()
    {
        var handler = new CapturingHandler
        {
            OnSend = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("boom", Encoding.UTF8)
            }
        };

        var client = CreateClient(handler);
        var res = await client.SendAsync(new SendRequest(["0400111222"], "Hello"));
        Assert.True(res.IsT1);
        Assert.Equal("500", res.AsT1.Code);
    }

    [Fact]
    public async Task SendAsync_Timeout_Returns_TimeoutError()
    {
        var handler = new CapturingHandler
        {
            OnThrow = _ => new TaskCanceledException("simulated timeout")
        };

        var client = CreateClient(handler);
        var res = await client.SendAsync(new SendRequest(["0400111222"], "Hello"));
        Assert.True(res.IsT1);
        Assert.Equal("Timeout", res.AsT1.Code);
    }

    [Fact]
    public async Task SendAsync_Validates_MaxSplit_From_And_Recipients()
    {
        var client = CreateClient(new CapturingHandler());

        // maxsplit out of range
        var r1 = await client.SendAsync(new SendRequest(["0400"], "hi", MaxSplit: 6));
        Assert.True(r1.IsT1);
        Assert.Equal("Validation", r1.AsT1.Code);

        // from > 11 chars
        var r2 = await client.SendAsync(new SendRequest(["0400"], "hi", From: "TOO-LONG-SENDER"));
        Assert.True(r2.IsT1);
        Assert.Equal("Validation", r2.AsT1.Code);

        // no recipients
        var r3 = await client.SendAsync(new SendRequest([], "hi"));
        Assert.True(r3.IsT1);
        Assert.Equal("Validation", r3.AsT1.Code);
    }

    [Fact]
    public async Task SendAsync_Encodes_Newlines_And_Joins_Recipients()
    {
        var handler = new CapturingHandler
        {
            OnSend = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("OK: 61400000000:1\nOK: 61400000001:2\n", Encoding.UTF8)
            }
        };

        var client = CreateClient(handler);
        var req = new SendRequest(
            To: ["61400000000", "61400000001"],
            Message: "line1\nline2",
            From: "Structura",
            Reference: "SampleRef-1",
            MaxSplit: 3,
            DelayMinutes: 1
        );

        var res = await client.SendAsync(req);
        Assert.True(res.IsT0);

        var body = handler.LastRequestBody!;
        Assert.Contains("to=61400000000%2C61400000001", body); // joined + URL-encoded comma
        Assert.Contains("message=line1%0Aline2", body);        // newline encoded
        Assert.Contains("from=Structura", body);
        Assert.Contains("ref=SampleRef-1", body);
        Assert.Contains("maxsplit=3", body);
        Assert.Contains("delay=1", body);
    }

    [Fact]
    public async Task GetBalanceAsync_Parses_Ok()
    {
        var handler = new CapturingHandler
        {
            OnSend = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("OK: 5000\n", Encoding.UTF8)
            }
        };
        var client = CreateClient(handler);
        var res = await client.GetBalanceAsync();
        Assert.True(res.IsT0);
        Assert.Equal(5000, res.AsT0);
    }

    [Fact]
    public async Task GetBalanceAsync_Error_Line_Returns_Error()
    {
        var handler = new CapturingHandler
        {
            OnSend = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ERROR: bad auth\n", Encoding.UTF8)
            }
        };
        var client = CreateClient(handler);
        var res = await client.GetBalanceAsync();
        Assert.True(res.IsT1);
        Assert.Equal("ERROR", res.AsT1.Code);
    }
}
