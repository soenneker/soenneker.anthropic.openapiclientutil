using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Anthropic.OpenApiClientUtil.Tests;

internal sealed class RecordingHandler : HttpMessageHandler
{
    public List<string> Urls { get; } = [];
    public List<string?> ApiKeys { get; } = [];
    public List<string?> Versions { get; } = [];
    public List<string?> Betas { get; } = [];
    public string? Body { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Urls.Add(request.RequestUri!.AbsoluteUri);
        ApiKeys.Add(request.Headers.TryGetValues("x-api-key", out var keys) ? keys.Single() : null);
        Versions.Add(request.Headers.TryGetValues("anthropic-version", out var versions) ? versions.Single() : null);
        Betas.Add(request.Headers.TryGetValues("anthropic-beta", out var betas) ? betas.Single() : null);
        Body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":"msg_test","type":"message","role":"assistant","model":"test-model","content":[{"type":"text","text":"hello"}],"data":[]}""",
                System.Text.Encoding.UTF8, "application/json")
        };
    }
}
