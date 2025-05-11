using Microsoft.Extensions.Logging;
using TVDataHub.Application.Queues;
using TVDataHub.Core.Domain.Entity;
using TVDataHub.Core.Repository;
using TVDataHub.Core.Scraper;
using TVDataHub.Core.Types;

namespace TVDataHub.Core.UseCase;

public interface IUpsertTVShowUseCase
{
    Task ExecuteAsync(TVShowId tvShowId);
}

internal sealed class UpsertTVShowUseCase(
    ITVMazeScraperService tvMazeScraperService,
    ITVShowRepository tvShowRepository,
    ILogger<UpsertTVShowUseCase> logger) : IUpsertTVShowUseCase
{
    public async Task ExecuteAsync(TVShowId tvShowId)
    {
        var value = tvShowId.Value;
        logger.LogDebug("Processing TVShow ID {TvShowId}", value);

        try
        {
            var tvShowDto = await tvMazeScraperService.GetTVShowAsync(value);

            if (tvShowDto is null)
            {
                logger.LogWarning("TVShow ID {TvShowId} could not be fetched. Skipping.", value);
            }

            var tvShow = new TVShow
            {
                Id = tvShowDto.Id,
                Name = tvShowDto.Name,
                Genres = tvShowDto.Genres,
                Premiered = tvShowDto.Premiered,
                Ended = tvShowDto.Ended,
                Updated = tvShowDto.Updated,
                Cast = tvShowDto.Cast.Select(dto => new Person
                {
                    Id = dto.Person.Id,
                    Name = dto.Person.Name,
                    Birthday = dto.Person.Birthday
                }).ToList()
            };

            await tvShowRepository.UpsertTVShowWithCast(tvShow);

            logger.LogInformation("Successfully upserted TVShow ID {TvShowId}", value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process TVShow ID {TvShowId}.", value);
            throw;
        }
    }
}