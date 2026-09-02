using Microsoft.Extensions.Options;

namespace GitHubExplorer.Web.ApiClients;

public class ApiKeyHandler : DelegatingHandler
{
    private ApiOptions _options;

    public ApiKeyHandler(IOptions<ApiOptions> options) => _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        request.Headers.Add(_options.ApiKeyHeaderName, _options.ApiKey);
        return await base.SendAsync(request, cancellationToken);
    }
}
