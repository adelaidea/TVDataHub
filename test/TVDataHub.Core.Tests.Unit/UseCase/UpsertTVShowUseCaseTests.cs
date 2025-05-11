using Microsoft.Extensions.Logging;
using Moq;
using TVDataHub.Core.Domain.Entity;
using TVDataHub.Core.Dto;
using TVDataHub.Core.Repository;
using TVDataHub.Core.Scraper;
using TVDataHub.Core.Scraper.Dto;
using TVDataHub.Core.Tests.Unit.Extensions;
using TVDataHub.Core.Types;
using TVDataHub.Core.UseCase;

namespace TVDataHub.Core.Tests.Unit.UseCase;

public class UpsertTVShowUseCaseTests
{
    private readonly Mock<ITVMazeScraperService> _tvMazeScraperServiceMock;
    private readonly Mock<ITVShowRepository> _tvShowRepositoryMock;
    private readonly Mock<ILogger<UpsertTVShowUseCase>> _loggerMock;
    private readonly UpsertTVShowUseCase _useCase;

    public UpsertTVShowUseCaseTests()
    {
        _tvMazeScraperServiceMock = new Mock<ITVMazeScraperService>();
        _tvShowRepositoryMock = new Mock<ITVShowRepository>();
        _loggerMock = new Mock<ILogger<UpsertTVShowUseCase>>();

        _useCase = new UpsertTVShowUseCase(
            _tvMazeScraperServiceMock.Object,
            _tvShowRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GivenAValidTVShowId_WhenUpseting_ThenLogsAndUpsertsSuccessfully()
    {
        // Arrange
        var tvShowId = new TVShowId(1);
        var tvShowDto = BuildTVShowDto();

        _tvMazeScraperServiceMock
            .Setup(service => service.GetTVShowAsync(tvShowId.Value))
            .ReturnsAsync(tvShowDto);

        _tvShowRepositoryMock
            .Setup(s => s.UpsertTVShowWithCast(It.IsAny<TVShow>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(tvShowId);

        // Assert
        _tvMazeScraperServiceMock.Verify(service => service.GetTVShowAsync(tvShowId.Value), Times.Once);
        _tvShowRepositoryMock.Verify(repo => repo.UpsertTVShowWithCast(It.Is<TVShow>(tv => tv.Id == tvShowDto.Id)),
            Times.Once);
        
        _loggerMock.VerifyLogInfo($"Successfully upserted TVShow ID {tvShowId.Value}", Times.Once);
    }

    [Fact]
    public async Task GivenNullTVShow_WhenUpserting_ThenLogsWarningAndDoesNotUpsert()
    {
        // Arrange
        var tvShowId = new TVShowId(1);
        
        _tvMazeScraperServiceMock
            .Setup(service => service.GetTVShowAsync(tvShowId.Value))
            .ReturnsAsync((TVMazeShowDto)null);

        // Act
        await _useCase.ExecuteAsync(tvShowId);

        // Assert
        _tvMazeScraperServiceMock.Verify(s => s.GetTVShowAsync(tvShowId.Value), Times.Once);
        _tvShowRepositoryMock.Verify(r => r.UpsertTVShowWithCast(It.IsAny<TVShow>()), Times.Never);
        
        _loggerMock.VerifyLogWarning($"TVShow ID {tvShowId.Value} could not be fetched. Skipping.", Times.Once);
    }

    [Fact]
    public async Task GivenAnExceptionInScraper_WhenUpserting_ThenLogsErrorAndThrowsException()
    {
        // Arrange
        var tvShowId = new TVShowId(1);
        var exception = new Exception("Scraper error");
        
        _tvMazeScraperServiceMock
            .Setup(s => s.GetTVShowAsync(tvShowId.Value))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(tvShowId));

        _loggerMock.VerifyLogError($"Failed to process TVShow ID {tvShowId.Value}.", exception, Times.Once);
        Assert.Equal("Scraper error", ex.Message);
    }

    private static TVMazeShowDto BuildTVShowDto()
    {
        return new TVMazeShowDto
        {
            Id = 1,
            Name = "Test Show",
            Genres = ["Drama", "Action", "Adventure"], // More genres for variety
            Premiered = new DateOnly(2020, 1, 1),
            Ended = new DateOnly(2022, 5, 15),
            Updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), // Current timestamp as the update time
            Embedded = new EmbeddedDto
            {
                Cast = BuildCast().ToList()
            }
        };
    }

    private static TVMazeCastDto[] BuildCast() =>
    [
        CreateCastMember(0, "Alice", new DateOnly(1985, 5, 1), null, "Female"),
        CreateCastMember(1, "Bob", new DateOnly(1990, 7, 15), null, "Male"),
        CreateCastMember(2, "Charlie", new DateOnly(1975, 10, 10), new DateOnly(2022, 8, 20), "Male")
    ];

    private static TVMazeCastDto CreateCastMember(int id, string name, DateOnly birthday, DateOnly? deathday,
        string gender) =>
        new()
        {
            Person = new TVMazePersonDto
            {
                Id = id,
                Name = name,
                Birthday = birthday,
                Deathday = deathday,
                Gender = gender
            }
        };
}