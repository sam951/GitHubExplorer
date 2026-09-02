using System.Text.Json.Serialization;

namespace GitHubExplorer.Api.GitHub;

public sealed class GitHubSearchResponse
{
    [JsonPropertyName("total_count")]public int TotalCount { get; set; }
    [JsonPropertyName("items")]public List<GitHubRepository> Items { get; set; } = new();
}
public sealed class GitHubRepository
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("full_name")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("owner")] public GitHubOwner Owner { get; set; } = new();
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("stargazers_count")] public int Stars { get; set; }
}
public sealed class GitHubOwner
{
    [JsonPropertyName("login")] public string Login { get; set; } = string.Empty;
}
