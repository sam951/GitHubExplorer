using GitHubExplorer.Api.GitHub;
using GitHubExplorer.Contracts.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class RepositoriesController : ControllerBase
    {
        private readonly IGitHubClient _gitHub;

        public RepositoriesController(IGitHubClient gitHub) => _gitHub = gitHub;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RepositoryDto>>> Search([FromQuery] string q, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Il parametro di ricerca 'q' è obbligatorio.");

            var results = await _gitHub.SearchRepositoriesAsync(q, ct);
            return Ok(results);
        }
    }
}
