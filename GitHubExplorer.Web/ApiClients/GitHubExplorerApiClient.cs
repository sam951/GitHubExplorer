using GitHubExplorer.Contracts.DTO;
using System.Net;

namespace GitHubExplorer.Web.ApiClients
{
    public sealed class GitHubExplorerApiClient : IGitHubExplorerApiClient
    {
        private readonly HttpClient _http;
        public GitHubExplorerApiClient(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<RepositoryDto>> SearchAsync(string q, CancellationToken ct)
        {
            var result = await _http.GetFromJsonAsync<List<RepositoryDto>>(
                $"api/repositories?q={Uri.EscapeDataString(q)}", ct);
            return result ?? [];
        }

        public async Task<IReadOnlyList<FavoriteDto>> GetFavoritesAsync(CancellationToken ct)
            => await _http.GetFromJsonAsync<List<FavoriteDto>>("api/favorites", ct) ?? [];

        public async Task<bool> AddFavoriteAsync(CreateFavoriteRequest request, CancellationToken ct)
        {
            var response = await _http.PostAsJsonAsync("api/favorites", request, ct);
            if (response.StatusCode == HttpStatusCode.Conflict) return false;   // 409 = duplicato
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> DeleteFavoriteAsync(int id, CancellationToken ct)
        {
            var response = await _http.DeleteAsync($"api/favorites/{id}", ct);
            if (response.StatusCode == HttpStatusCode.NotFound) return false;
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> UpdateNoteAsync(int id, string? note, CancellationToken ct)
        {
            var response = await _http.PutAsJsonAsync($"api/favorites/{id}/note", note, ct);
            if (response.StatusCode == HttpStatusCode.NotFound) return false;
            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}
