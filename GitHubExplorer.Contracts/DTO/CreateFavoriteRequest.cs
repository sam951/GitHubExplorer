using System.ComponentModel.DataAnnotations;

namespace GitHubExplorer.Contracts.DTO;

public record CreateFavoriteRequest(
        long GithubId,
        [property: Required] string Name,
        [property: Required] string FullName,
        [property: Required] string Owner,
        [property: Required] string HtmlUrl,
        string? Description,
        int Stars,
        string? Note);
