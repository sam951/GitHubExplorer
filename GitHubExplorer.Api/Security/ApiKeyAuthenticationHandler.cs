using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace GitHubExplorer.Api.Security;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ApiKeyOptions _apiKeyOptions;

    public ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder urlEncoder,
        IOptions<ApiKeyOptions> apiKeyOptions) : base(options, logger, urlEncoder)
    {
        _apiKeyOptions = apiKeyOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // header assente: nessuna credenziale da validare
        if (!Request.Headers.TryGetValue(_apiKeyOptions.HeaderName, out var provided) || string.IsNullOrWhiteSpace(provided))
            return Task.FromResult(AuthenticateResult.NoResult());

        // chiave presente ma non valida
        var providedKey = Encoding.UTF8.GetBytes(provided.ToString());
        var expectedKey = Encoding.UTF8.GetBytes(_apiKeyOptions.Key);

        if (!CryptographicOperations.FixedTimeEquals(providedKey, expectedKey))
            return Task.FromResult(AuthenticateResult.Fail("API Key non valida."));

        // chiave valida, creo l'identità
        var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyClient") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
