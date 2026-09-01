using GitHubExplorer.Contracts.DTO;

namespace GitHubExplorer.Api.GitHub
{
    public interface IGitHubClient
    {
        Task<IReadOnlyList<RepositoryDto>> SearchRepositoriesAsync(string query, CancellationToken ct);
    }
}
