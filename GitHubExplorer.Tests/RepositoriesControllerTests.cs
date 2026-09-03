using GitHubExplorer.Api.Controllers;
using GitHubExplorer.Api.GitHub;
using GitHubExplorer.Contracts.DTO;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Tests;

[TestClass]
public class RepositoriesControllerTests
{
    private sealed class SpyGitHubClient : IGitHubClient
    {
        public bool Called;
        public int Page, PerPage;
        public string? Sort;

        public Task<PagedResult<RepositoryDto>> SearchRepositoriesAsync(
            string query, int page, int perPage, string? sort, CancellationToken ct)
        {
            Called = true; Page = page; PerPage = perPage; Sort = sort;
            return Task.FromResult(new PagedResult<RepositoryDto>(Array.Empty<RepositoryDto>(), page, perPage, 0));
        }
    }

    private static (RepositoriesController controller, SpyGitHubClient spy) Build()
    {
        var spy = new SpyGitHubClient();
        return (new RepositoriesController(spy), spy);
    }

    [TestMethod]
    public async Task Query_vuota_restituisce_BadRequest_e_non_chiama_GitHub()
    {
        var (controller, spy) = Build();
        var result = await controller.Search(q: "  ");
        Assert.IsInstanceOfType<BadRequestObjectResult>(result.Result);
        Assert.IsFalse(spy.Called, "Con q vuota non deve interrogare GitHub");
    }

    [TestMethod]
    public async Task PerPage_oltre_il_massimo_viene_limitato_a_100()
    {
        var (controller, spy) = Build();
        await controller.Search(q: "blazor", perPage: 500);
        Assert.AreEqual(100, spy.PerPage);
    }

    [TestMethod]
    public async Task PerPage_sotto_il_minimo_viene_portato_a_1()
    {
        var (controller, spy) = Build();
        await controller.Search(q: "blazor", perPage: 0);
        Assert.AreEqual(1, spy.PerPage);
    }

    [TestMethod]
    public async Task Pagina_negativa_viene_portata_a_1()
    {
        var (controller, spy) = Build();
        await controller.Search(q: "blazor", page: -5);
        Assert.AreEqual(1, spy.Page);
    }

    [TestMethod]
    public async Task Sort_non_ammesso_viene_ignorato()
    {
        var (controller, spy) = Build();
        await controller.Search(q: "blazor", sort: "pippo");
        Assert.IsNull(spy.Sort);
    }

    [TestMethod]
    public async Task Sort_ammesso_viene_passato_a_GitHub()
    {
        var (controller, spy) = Build();
        await controller.Search(q: "blazor", sort: "stars");
        Assert.AreEqual("stars", spy.Sort);
    }
}
