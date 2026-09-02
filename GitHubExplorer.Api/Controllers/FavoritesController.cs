using GitHubExplorer.Api.Data;
using GitHubExplorer.Contracts.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class FavoritesController : ControllerBase
{
    private readonly IFavoritesRepository _repository;

    public FavoritesController(IFavoritesRepository repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FavoriteDto>>> GetAll(CancellationToken ct)
        => Ok(await _repository.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateFavoriteRequest request, CancellationToken ct)
    {
        if (await _repository.ExistsAsync(request.GithubId, ct))
            return Conflict($"Il repository '{request.FullName}' è già nei preferiti.");

        var newId = await _repository.AddAsync(request, ct);
        return Created($"/api/favorites/{newId}", new { id = newId });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _repository.DeleteAsync(id, ct) ? NoContent() : NotFound();

    [HttpPut("{id:int}/note")]
    public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateNoteRequest request, CancellationToken ct)
        => await _repository.UpdateNoteAsync(id, request.Note, ct) ? NoContent() : NotFound();
}
