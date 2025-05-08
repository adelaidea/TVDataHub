using Microsoft.Extensions.Logging;
using TVDataHub.Application.Dto;
using TVDataHub.Domain.Repository;

namespace TVDataHub.Application.UseCase;

public interface IGetPaginatedTVShowsUseCase
{ 
    Task<IReadOnlyCollection<TVShowDto>> ExecuteAsync(int page = 1);
}

internal sealed class GetPaginatedTVShowsUseCase(
    ITVShowRepository tvShowRepository,
    ILogger<GetPaginatedTVShowsUseCase> logger) : IGetPaginatedTVShowsUseCase
{
    private readonly int _pageSize = 10;
    
    public async Task<IReadOnlyCollection<TVShowDto>> ExecuteAsync(int page = 1)
    {
        try
        {
            var tvShows = await tvShowRepository.GetPaginated(_pageSize, page);

            return tvShows.Select(
                tvShow => new TVShowDto(
                    Id: tvShow.Id,
                    Name: tvShow.Name,
                    Genres: tvShow.Genres,
                    Premiered: tvShow.Premiered,
                    Ended: tvShow.Ended,
                    Cast: tvShow.Cast.OrderBy(c => c.Birthday)
                        .Select(c => new CastMemberDto(
                            c.Id, 
                            c.Name, 
                            c.Birthday)
                        ).ToList()
                )).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get paginated TVShows.");
            throw new ApplicationException("Failed to retrieve TVShows. Please try again later.");
        }
    }
}