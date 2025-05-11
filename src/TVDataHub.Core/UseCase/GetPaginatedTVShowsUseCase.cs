using Microsoft.Extensions.Logging;
using TVDataHub.Core.Dto;
using TVDataHub.Core.Repository;
using TVDataHub.Core.UseCase.Response;

namespace TVDataHub.Core.UseCase;

public interface IGetPaginatedTVShowsUseCase
{
    Task<PagedResult<TVShowDto>> ExecuteAsync(int page = 1);
}

internal sealed class GetPaginatedTVShowsUseCase(
    ITVShowRepository tvShowRepository,
    ILogger<GetPaginatedTVShowsUseCase> logger) : IGetPaginatedTVShowsUseCase
{
    private readonly int _pageSize = 10;

    public async Task<PagedResult<TVShowDto>> ExecuteAsync(int page = 1)
    {
        try
        {
            var tvShows = await tvShowRepository.GetPaginated(_pageSize, page);

            var dto = tvShows.Select(
                tvShow => new TVShowDto(
                    Id: tvShow.Id,
                    Name: tvShow.Name,
                    Genres: tvShow.Genres,
                    Premiered: tvShow.Premiered,
                    Ended: tvShow.Ended,
                    Cast: tvShow.Cast.OrderByDescending(c => c.Birthday)
                        .Select(c => new CastMemberDto(
                            c.Id,
                            c.Name,
                            c.Birthday)
                        ).ToList()
                )).ToList();

            var total = await tvShowRepository.GetTotal();
            
            return new PagedResult<TVShowDto> { Items = dto, TotalCount = total };

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get paginated TVShows.");
            throw new ApplicationException("Failed to retrieve TVShows. Please try again later.");
        }
    }
}