namespace GitHubExplorer.Api.GitHub;

public sealed class GitHubOptions
{
    public string BaseUrl { get; set; } = "https://api.github.com";
    public string UserAgent { get; set; } = "GitHubExplorer";
    public string? Token { get; set; }
}
