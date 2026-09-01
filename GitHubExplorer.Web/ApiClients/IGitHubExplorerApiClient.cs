using GitHubExplorer.Contracts.DTO;

namespace GitHubExplorer.Web.ApiClients
{
    public interface IGitHubExplorerApiClient
    {
        Task<IReadOnlyList<RepositoryDto>> SearchAsync(string query, CancellationToken ct);
        Task<IReadOnlyList<FavoriteDto>> GetFavoritesAsync(CancellationToken ct);
        Task<bool> AddFavoriteAsync(CreateFavoriteRequest request, CancellationToken ct);
        Task<bool> DeleteFavoriteAsync(int id, CancellationToken ct);
        Task<bool> UpdateNoteAsync(int id, string? note, CancellationToken ct);
    }
}
