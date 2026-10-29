using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Anthropic.HttpClients.Abstract;

namespace Soenneker.Anthropic.OpenApiClientUtil.Tests;

internal sealed class TestHttpClient(HttpClient client) : IAnthropicOpenApiHttpClient
{
    public ValueTask<HttpClient> Get(CancellationToken cancellationToken = default) => ValueTask.FromResult(client);
    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

