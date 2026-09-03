using GitHubExplorer.Contracts.DTO;

namespace GitHubExplorer.Web.Services;

public sealed class SearchState
{
    public string Query { get; set; } = "";
    public string LastQuery { get; set; } = "";
    public string Sort { get; set; } = "";
    public int Page { get; set; } = 1;
    public bool GridView { get; set; }
    public PagedResult<RepositoryDto>? Results { get; set; }
}
