using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Soenneker.Anthropic.OpenApiClient.Models;

namespace Soenneker.Anthropic.OpenApiClientUtil.Tests;

public sealed class AnthropicOpenApiClientUtilTests
{
    private static IConfiguration Configuration() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Anthropic:ApiKey"] = "first",
        ["Anthropic:Beta"] = "test-beta"
    }).Build();

    [Test]
    public async Task UsesCurrentKeyVersionAndConfiguredBaseUrl()
    {
        IConfiguration configuration = Configuration();
        using var handler = new RecordingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.anthropic.com/") };
        await using var utility = new AnthropicOpenApiClientUtil(new TestHttpClient(http), configuration);
        var client = await utility.Get();
        if (!ReferenceEquals(client, await utility.Get())) throw new Exception("Client was not cached.");
        await client.V1.Models.GetAsync();
        configuration["Anthropic:ApiKey"] = "rotated";
        await client.V1.Models.GetAsync();
        if (handler.Urls[0] != "https://api.anthropic.com/v1/models" || handler.ApiKeys[0] != "first" ||
            handler.ApiKeys[1] != "rotated" || handler.Versions[0] != "2026-10-21" || handler.Betas[0] != "test-beta")
            throw new Exception("The request URL or Anthropic headers were incorrect.");
    }

    [Test]
    public async Task PreservesPerRequestHeadersAndBetaPagination()
    {
        using var handler = new RecordingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.anthropic.com/") };
        await using var utility = new AnthropicOpenApiClientUtil(new TestHttpClient(http), Configuration());
        var client = await utility.Get();
        await client.V1.ModelsBetaTrue.GetAsync(request =>
        {
            request.QueryParameters.Limit = 2;
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Headers.Add("anthropic-beta", "per-request");
        });
        if (handler.Urls[0] != "https://api.anthropic.com/v1/models?beta=true&limit=2" ||
            handler.Versions[0] != "2023-06-01" || handler.Betas[0] != "per-request")
            throw new Exception("Beta pagination or per-request headers were not preserved.");
    }

    [Test]
    public async Task SerializesMessageAndReadsTypedResponse()
    {
        using var handler = new RecordingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.anthropic.com/") };
        await using var utility = new AnthropicOpenApiClientUtil(new TestHttpClient(http), Configuration());
        var client = await utility.Get();
        var response = await client.V1.Messages.PostAsync(new CreateMessageParams { Model = "test-model", MaxTokens = 16, Messages = [], Stream = false });
        using JsonDocument body = JsonDocument.Parse(handler.Body!);
        if (handler.Urls[0] != "https://api.anthropic.com/v1/messages" || body.RootElement.GetProperty("max_tokens").GetInt32() != 16 ||
            body.RootElement.GetProperty("model").GetString() != "test-model" || response?.Id != "msg_test")
            throw new Exception("Message serialization or deserialization failed.");
    }

    [Test]
    public async Task RejectsUntrustedHostsAndMissingKeys()
    {
        IConfiguration configuration = Configuration();
        using var handler = new RecordingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.anthropic.com/") };
        await using var utility = new AnthropicOpenApiClientUtil(new TestHttpClient(http), configuration);
        var client = await utility.Get();
        bool hostRejected = false;
        try { await client.V1.Models.WithUrl("https://other.example/v1/models").GetAsync(); }
        catch (InvalidOperationException) { hostRejected = true; }
        if (!hostRejected || handler.Urls.Count != 0) throw new Exception("Untrusted host was accepted.");

        configuration["Anthropic:ApiKey"] = "";
        try { await client.V1.Models.GetAsync(); }
        catch (InvalidOperationException)
        {
            if (handler.Urls.Count != 0) throw new Exception("A request was sent without an API key.");
            return;
        }
        throw new Exception("Missing API key was accepted.");
    }
}
