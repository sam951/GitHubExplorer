using System.ComponentModel.DataAnnotations;

namespace GitHubExplorer.Contracts.DTO;

public record CreateFavoriteRequest(
    long GithubId,
    [Required] string Name,
    [Required] string FullName,
    [Required] string Owner,
    [Required] string HtmlUrl,
    string? Description,
    int Stars,
    string? Note);
