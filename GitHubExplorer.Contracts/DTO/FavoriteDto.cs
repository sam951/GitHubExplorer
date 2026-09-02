namespace GitHubExplorer.Contracts.DTO;

public record FavoriteDto(int Id, long GithubId, string Name, string FullName, string Owner, string HtmlUrl, string? Description, int Stars, string? Note, DateTime CreatedAt);
