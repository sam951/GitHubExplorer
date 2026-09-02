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
    private readonly IGitHubClient _gitHub;

    public RepositoriesController(IGitHubClient gitHub) => _gitHub = gitHub;

    [HttpGet]
    public async Task<ActionResult<PagedResult<RepositoryDto>>> Search(
        [FromQuery] string q, 
        [FromQuery] int page = 1, 
        [FromQuery] int perPage = 10, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Il parametro di ricerca 'q' è obbligatorio.");

        page = Math.Max(1, page);
        perPage = Math.Clamp(perPage, 1, 100);

        var results = await _gitHub.SearchRepositoriesAsync(q, page, perPage, ct);
        return Ok(results);
    }
}
