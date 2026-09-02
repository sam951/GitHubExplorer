namespace GitHubExplorer.Api.Security;

public sealed class ApiKeyOptions
{
    public string HeaderName { get; set; } = "X-Api-Key";
    public string Key { get; set; } = "";
}
