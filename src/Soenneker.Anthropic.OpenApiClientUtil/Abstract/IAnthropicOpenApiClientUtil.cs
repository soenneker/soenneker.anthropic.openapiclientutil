using Soenneker.Anthropic.OpenApiClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Anthropic.OpenApiClientUtil.Abstract;

/// <summary>
/// Exposes a cached OpenAPI client instance.
/// </summary>
public interface IAnthropicOpenApiClientUtil: IDisposable, IAsyncDisposable
{
    /// <summary>Gets the cached Anthropic client. Reads Anthropic:ApiKey for each request.
    /// Anthropic:Version defaults to 2026-10-21; Anthropic:Beta optionally supplies beta headers.
    /// Per-request version and beta headers take precedence. Configure Anthropic:ClientBaseUrl before first use.</summary>
    ValueTask<AnthropicOpenApiClient> Get(CancellationToken cancellationToken = default);
}
