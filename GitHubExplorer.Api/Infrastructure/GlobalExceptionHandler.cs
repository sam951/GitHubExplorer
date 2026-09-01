using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace GitHubExplorer.Api.Infrastructure
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            var (status, title) = Map(exception);

            _logger.LogError(exception, "Errore non gestito: {Message}", exception.Message);

            context.Response.StatusCode = status;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title
                }
            });
        }

        private static (int status, string title) Map(Exception exception) => exception switch
        {
            MySqlException { Number: 1062 } =>
                (StatusCodes.Status409Conflict, "Il repository è già nei preferiti."),

            HttpRequestException =>
                (StatusCodes.Status502BadGateway, "Errore nel contattare GitHub. Riprova più tardi."),

            _ =>
                (StatusCodes.Status500InternalServerError, "Si è verificato un errore imprevisto.")
        };
    }
}
