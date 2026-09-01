namespace GitHubExplorer.Contracts.DTO
{
    public record CreateFavoriteRequest(long GithubId, string Name, string FullName, string Owner, string HtmlUrl, string? Description, int Stars, string? Note);
}
