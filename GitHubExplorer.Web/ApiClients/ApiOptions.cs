namespace GitHubExplorer.Web.ApiClients
{
    public sealed class ApiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKeyHeaderName { get; set; } = "X-Api-Key";
        public string ApiKey { get; set; } = string.Empty;
    }
}
