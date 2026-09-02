using GitHubExplorer.Api.Controllers;
using GitHubExplorer.Api.Data;
using GitHubExplorer.Contracts.DTO;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Tests;

[TestClass]
public class FavoritesControllerTests
{    
    private sealed class FakeFavoritesRepository : IFavoritesRepository
    {
        public bool Exists { get; set; }
        public bool AddCalled { get; private set; }

        public Task<bool> ExistsAsync(long githubId, CancellationToken ct) => Task.FromResult(Exists);
        public Task<int> AddAsync(CreateFavoriteRequest request, CancellationToken ct)
        {
            AddCalled = true;
            return Task.FromResult(1);
        }
        public Task<IReadOnlyList<FavoriteDto>> GetAllAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<FavoriteDto>>([]);
        public Task<bool> DeleteAsync(int id, CancellationToken ct) => Task.FromResult(true);
        public Task<bool> UpdateNoteAsync(int id, string? note, CancellationToken ct) => Task.FromResult(true);
    }

    private static CreateFavoriteRequest Sample() =>
        new(GithubId: 1, Name: "x", FullName: "o/x", Owner: "o",
            HtmlUrl: "https://github.com/o/x", Description: null, Stars: 0, Note: null);

    [TestMethod]
    public async Task Se_gia_presente_add_restituisce_Conflict()
    {
        var repo = new FakeFavoritesRepository { Exists = true };
        var controller = new FavoritesController(repo);

        var result = await controller.Add(Sample(), CancellationToken.None);

        Assert.IsInstanceOfType<ConflictObjectResult>(result);
        Assert.IsFalse(repo.AddCalled, "Non deve inserire un duplicato");
    }

    [TestMethod]
    public async Task Se_aggiungo_nuovo_preferito_restituisce_Created()
    {
        var repo = new FakeFavoritesRepository { Exists = false };
        var controller = new FavoritesController(repo);

        var result = await controller.Add(Sample(), CancellationToken.None);

        Assert.IsInstanceOfType<CreatedResult>(result);
        Assert.IsTrue(repo.AddCalled);
    }
}