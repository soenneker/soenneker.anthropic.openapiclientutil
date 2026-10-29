using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Soenneker.Anthropic.OpenApiClientUtil;

internal sealed class AnthropicAuthenticationProvider(IConfiguration configuration, Uri baseAddress) : IAuthenticationProvider
{
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Uri uri = request.URI;
        if (uri.Scheme != Uri.UriSchemeHttps || !uri.Authority.Equals(baseAddress.Authority, StringComparison.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.StartsWith(baseAddress.AbsolutePath.TrimEnd('/') + "/", StringComparison.Ordinal))
            throw new InvalidOperationException("Anthropic credentials may only be sent to the configured HTTPS API base URL.");

        string? apiKey = configuration["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Anthropic:ApiKey is required.");

        request.Headers.Remove("x-api-key");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.TryAdd("anthropic-version", configuration["Anthropic:Version"] ?? "2026-10-21");

        string? beta = configuration["Anthropic:Beta"];
        if (!string.IsNullOrWhiteSpace(beta))
            request.Headers.TryAdd("anthropic-beta", beta);

        return Task.CompletedTask;
    }
}
