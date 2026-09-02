using GitHubExplorer.Api.GitHub;
using GitHubExplorer.Contracts.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class RepositoriesController : ControllerBase
{
    private static readonly string[] AllowedSorts = { "stars", "forks", "help-wanted-issues", "updated" };

    private readonly IGitHubClient _gitHub;

    public RepositoriesController(IGitHubClient gitHub) => _gitHub = gitHub;

    [HttpGet]
    public async Task<ActionResult<PagedResult<RepositoryDto>>> Search(
        [FromQuery] string q, 
        [FromQuery] int page = 1, 
        [FromQuery] int perPage = 10, 
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Il parametro di ricerca 'q' è obbligatorio.");

        page = Math.Max(1, page);
        perPage = Math.Clamp(perPage, 1, 100);
        if (sort is not null && !AllowedSorts.Contains(sort)) sort = null;

        var results = await _gitHub.SearchRepositoriesAsync(q, page, perPage, sort, ct);
        return Ok(results);
    }
}
