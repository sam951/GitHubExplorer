using GitHubExplorer.Contracts.DTO;

namespace GitHubExplorer.Api.GitHub;

public class GitHubClient : IGitHubClient
{
    private readonly HttpClient _http;
    public GitHubClient(HttpClient http) => _http = http;
    public async Task<PagedResult<RepositoryDto>> SearchRepositoriesAsync(string query, int page, int perPage, string? sort, CancellationToken ct)
    {
        var url = $"search/repositories?q={Uri.EscapeDataString(query)}&page={page}&per_page={perPage}";
        if (!string.IsNullOrWhiteSpace(sort))
            url += $"&sort={Uri.EscapeDataString(sort)}&order=desc";

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GitHubSearchResponse>(ct);
        if (payload is null)
            return new PagedResult<RepositoryDto>(Array.Empty<RepositoryDto>(), page, perPage, 0);

        var items = payload.Items
        .Select(r => new RepositoryDto(
            GithubId: r.Id,
            Name: r.Name,
            FullName: r.FullName,
            Owner: r.Owner?.Login ?? "",
            HtmlUrl: r.HtmlUrl,
            Description: r.Description,
            Stars: r.Stars,
            Language: r.Language,
            Forks: r.Forks,
            UpdatedAt: r.UpdatedAt))
        .ToList();

        var total = Math.Min(payload.TotalCount, 1000);
        return new PagedResult<RepositoryDto>(items, page, perPage, total);
    }
}
