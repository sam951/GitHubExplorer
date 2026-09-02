using GitHubExplorer.Api.GitHub;

namespace GitHubExplorer.Tests;

[TestClass]
[TestCategory("Integration")]
public class GitHubClientIntegrationTests
{
    [TestMethod]
    public async Task Se_cerco_blazor_ottengo_risultati()
    {
        using var http = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("GitHubExplorer");
        http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

        var client = new GitHubClient(http);

        var result = await client.SearchRepositoriesAsync("blazor", page: 1, perPage: 10, CancellationToken.None);

        Assert.IsNotEmpty(result.Items, "La ricerca deve restituire risultati");
        Assert.IsTrue(result.Items.All(r => !string.IsNullOrWhiteSpace(r.Name)), "Ogni repo deve avere un nome");
        Assert.IsGreaterThan(0, result.TotalCount, "Il totale deve essere valorizzato");

        foreach (var r in result.Items.Take(5))
            Console.WriteLine($"{r.FullName} ⭐{r.Stars} — {r.Description}");
    }
}