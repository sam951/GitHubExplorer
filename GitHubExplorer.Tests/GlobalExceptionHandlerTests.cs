using GitHubExplorer.Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace GitHubExplorer.Tests;

[TestClass]
public class GlobalExceptionHandlerTests
{
    private sealed class FakeProblemDetailsService : IProblemDetailsService
    {
        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context) => ValueTask.FromResult(true);
        public ValueTask WriteAsync(ProblemDetailsContext context) => ValueTask.CompletedTask;
    }

    private static async Task<int> StatusFor(Exception ex)
    {
        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance, new FakeProblemDetailsService());
        var ctx = new DefaultHttpContext();
        await handler.TryHandleAsync(ctx, ex, CancellationToken.None);
        return ctx.Response.StatusCode;
    }

    [TestMethod]
    public async Task GitHub_irraggiungibile_diventa_502()
        => Assert.AreEqual(StatusCodes.Status502BadGateway, await StatusFor(new HttpRequestException()));

    [TestMethod]
    public async Task Eccezione_generica_diventa_500()
        => Assert.AreEqual(StatusCodes.Status500InternalServerError, await StatusFor(new InvalidOperationException()));
}
