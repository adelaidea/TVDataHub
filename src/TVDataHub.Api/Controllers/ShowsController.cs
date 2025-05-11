using Microsoft.AspNetCore.Mvc;
using TVDataHub.Core.Dto;
using TVDataHub.Core.UseCase;

namespace TVDataHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TVShowsController(
    IGetPaginatedTVShowsUseCase getPaginatedTVShowsUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TVShowDto>>> Get([FromQuery] int page = 1)
    {
        var result = await getPaginatedTVShowsUseCase.ExecuteAsync(page);

        return Ok(result);
    }
}