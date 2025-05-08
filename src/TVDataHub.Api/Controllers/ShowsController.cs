using Microsoft.AspNetCore.Mvc;
using TVDataHub.Application.Dto;
using TVDataHub.Application.UseCase;

namespace TVDataHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TVShowsController(
    IGetPaginatedTVShowsUseCase getPaginatedTVShowsUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TVShowDto>>> Get([FromQuery] int page = 1)
    {
        var tvShowDtos = await getPaginatedTVShowsUseCase.ExecuteAsync(page);

        return Ok(tvShowDtos);
    }
}