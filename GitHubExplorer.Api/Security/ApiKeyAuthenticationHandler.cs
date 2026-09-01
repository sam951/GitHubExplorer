using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace GitHubExplorer.Api.Security
{
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
            // Scenario 1: header assente → nessuna credenziale fornita
            if (!Request.Headers.TryGetValue(_apiKeyOptions.HeaderName, out var provided) || string.IsNullOrWhiteSpace(provided))
                return Task.FromResult(AuthenticateResult.NoResult());

            // Scenario 2: header presente ma chiave sbagliata → fallimento
            if (!string.Equals(provided, _apiKeyOptions.Key, StringComparison.Ordinal))
                return Task.FromResult(AuthenticateResult.Fail("API Key non valida."));

            // Scenario 3: chiave giusta → identità autenticata
            var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyClient") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
