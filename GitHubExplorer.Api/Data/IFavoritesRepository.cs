using GitHubExplorer.Contracts.DTO;

namespace GitHubExplorer.Api.Data
{
    public interface IFavoritesRepository
    {
        Task<IReadOnlyList<FavoriteDto>> GetAllAsync(CancellationToken ct);
        Task<int> AddAsync(CreateFavoriteRequest request, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
        Task<bool> UpdateNoteAsync(int id, string? note, CancellationToken ct);
        Task<bool> ExistsAsync(long githubId, CancellationToken ct);
    }
}
