using GitHubExplorer.Contracts.DTO;
using System.Net;

namespace GitHubExplorer.Web.ApiClients;

public sealed class GitHubExplorerApiClient : IGitHubExplorerApiClient
{
    private readonly HttpClient _http;
    public GitHubExplorerApiClient(HttpClient http) => _http = http;

    public async Task<PagedResult<RepositoryDto>> SearchAsync(string q, int page, int perPage, string? sort, CancellationToken ct)
    {
        var url = $"api/repositories?q={Uri.EscapeDataString(q)}&page={page}&perPage={perPage}";
        if (!string.IsNullOrWhiteSpace(sort))
            url += $"&sort={Uri.EscapeDataString(sort)}";

        var result = await _http.GetFromJsonAsync<PagedResult<RepositoryDto>>(url, ct);
        return result ?? new PagedResult<RepositoryDto>([], page, perPage, 0);
    }

    public async Task<IReadOnlyList<FavoriteDto>> GetFavoritesAsync(CancellationToken ct)
        => await _http.GetFromJsonAsync<List<FavoriteDto>>("api/favorites", ct) ?? [];

    public async Task<bool> AddFavoriteAsync(CreateFavoriteRequest request, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync("api/favorites", request, ct);
        if (response.StatusCode == HttpStatusCode.Conflict) return false;
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
        var response = await _http.PutAsJsonAsync($"api/favorites/{id}/note", new UpdateNoteRequest(note), ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return false;
        response.EnsureSuccessStatusCode();
        return true;
    }
}
