using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Soenneker.Extensions.ValueTask;
using Soenneker.Anthropic.HttpClients.Abstract;
using Soenneker.Anthropic.OpenApiClientUtil.Abstract;
using Soenneker.Anthropic.OpenApiClient;
using Soenneker.Utils.AsyncSingleton;

namespace Soenneker.Anthropic.OpenApiClientUtil;

public sealed class AnthropicOpenApiClientUtil : IAnthropicOpenApiClientUtil
{
    private readonly AsyncSingleton<AnthropicOpenApiClient> _client;

    public AnthropicOpenApiClientUtil(IAnthropicOpenApiHttpClient httpClientUtil, IConfiguration configuration)
    {
        _client = new AsyncSingleton<AnthropicOpenApiClient>(async token =>
        {
            HttpClient httpClient = await httpClientUtil.Get(token).NoSync();

            Uri baseAddress = httpClient.BaseAddress ?? throw new InvalidOperationException("The Anthropic HTTP client requires a base address.");
            var authenticationProvider = new AnthropicAuthenticationProvider(configuration, baseAddress);
            var requestAdapter = new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient)
            {
                BaseUrl = baseAddress.AbsoluteUri.TrimEnd('/')
            };
            return new AnthropicOpenApiClient(requestAdapter);
        });
    }

    public ValueTask<AnthropicOpenApiClient> Get(CancellationToken cancellationToken = default)
    {
        return _client.Get(cancellationToken);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }
}

