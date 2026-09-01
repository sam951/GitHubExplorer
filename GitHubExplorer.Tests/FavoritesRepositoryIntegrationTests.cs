using GitHubExplorer.Api.Data;
using GitHubExplorer.Contracts.DTO;

namespace GitHubExplorer.Tests;

[TestClass]
[TestCategory("Integration")]
public class FavoritesRepositoryIntegrationTests
{
    private const string ConnString =
        "Server=localhost;Port=3306;Database=githubexplorer;User ID=ghexp_user;Password=ghexp_dev_pass;";

    [TestMethod]
    public async Task Add_Get_Delete_Cycle()
    {
        var repo = new FavoritesRepository(new MySqlConnectionFactory(ConnString));
        var ct = CancellationToken.None;

        var request = new CreateFavoriteRequest(
            GithubId: 999999,
            Name: "TestRepo",
            FullName: "sam951/TestRepo",
            Owner: "sam951",
            HtmlUrl: "https://github.com/sam951/TestRepo",
            Description: "repo di test",
            Stars: 42,
            Note: "nota di prova");

        var newId = await repo.AddAsync(request, ct);
        Assert.IsGreaterThan(0, newId, "L'INSERT deve restituire un id valido");

        var all = await repo.GetAllAsync(ct);
        Assert.IsTrue(all.Any(f => f.Id == newId && f.Name == "TestRepo"), "Il preferito deve comparire nella lista");

        var deleted = await repo.DeleteAsync(newId, ct);
        Assert.IsTrue(deleted);
    }
}
