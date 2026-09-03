using System.Text.Json.Serialization;

namespace GitHubExplorer.Api.GitHub;

public sealed class GitHubSearchResponse
{
    [JsonPropertyName("total_count")]public int TotalCount { get; set; }
    [JsonPropertyName("items")]public List<GitHubRepository> Items { get; set; } = new();
}
public sealed class GitHubRepository
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("full_name")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("owner")] public GitHubOwner Owner { get; set; } = new();
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("stargazers_count")] public int Stars { get; set; }
    [JsonPropertyName("language")] public string? Language { get; set; }
    [JsonPropertyName("forks_count")] public int Forks { get; set; }
    [JsonPropertyName("updated_at")] public DateTime UpdatedAt { get; set; }
}
public sealed class GitHubOwner
{
    [JsonPropertyName("login")] public string Login { get; set; } = string.Empty;
}
