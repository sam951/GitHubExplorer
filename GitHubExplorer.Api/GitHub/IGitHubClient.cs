using GitHubExplorer.Contracts.DTO;

namespace GitHubExplorer.Api.GitHub;

public interface IGitHubClient
{
    Task<PagedResult<RepositoryDto>> SearchRepositoriesAsync(string query, int page, int perPage, CancellationToken ct);
}
