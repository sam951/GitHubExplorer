using GitHubExplorer.Api.GitHub;

namespace GitHubExplorer.Tests;

[TestClass]
[TestCategory("Integration")]
public class GitHubClientIntegrationTests
{
    [TestMethod]
    public async Task Search_blazor_returns_results()
    {
        using var http = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("GitHubExplorer");
        http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

        var client = new GitHubClient(http);

        var results = await client.SearchRepositoriesAsync("blazor", CancellationToken.None);

        Assert.IsNotEmpty(results, "La ricerca deve restituire risultati");
        Assert.IsTrue(results.All(r => !string.IsNullOrWhiteSpace(r.Name)), "Ogni repo deve avere un nome");

        foreach (var r in results.Take(5))
            Console.WriteLine($"{r.FullName} ⭐{r.Stars} — {r.Description}");
    }
}