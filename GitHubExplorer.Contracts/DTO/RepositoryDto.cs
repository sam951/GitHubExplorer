namespace GitHubExplorer.Contracts.DTO;

public record RepositoryDto(long GithubId, string Name, string FullName, string Owner, string HtmlUrl, string? Description, int Stars);
