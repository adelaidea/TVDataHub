using Microsoft.Extensions.Logging;
using Moq;
using TVDataHub.Application.Queues;
using TVDataHub.Core.Repository;
using TVDataHub.Core.Scraper;
using TVDataHub.Core.Tests.Unit.Extensions;
using TVDataHub.Core.Types;
using TVDataHub.Core.UseCase;

namespace TVDataHub.Core.Tests.Unit.UseCase;

public class GetTVShowsToBeUpsertedUseCaseTests
{
    private readonly Mock<IStaticQueue<TVShowId>> _queueMock = new();
    private readonly Mock<ITVMazeScraperService> _tvMazeScraperServiceMock = new();
    private readonly Mock<ITVShowRepository> _tvShowRepositoryMock = new();
    private readonly Mock<ILogger<GetTVShowsToBeUpsertedUseCase>> _loggerMock = new();
    private readonly GetTVShowsToBeUpsertedUseCase _useCase;

    public GetTVShowsToBeUpsertedUseCaseTests()
    {
        _useCase = new GetTVShowsToBeUpsertedUseCase(
            _tvMazeScraperServiceMock.Object,
            _tvShowRepositoryMock.Object,
            _queueMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GivenMissingAndOutdatedTVShows_WhenGettingToBeUpserted_ThenLogsAppropriateMessages()
    {
        // Arrange
        var remoteUpdates = new Dictionary<int, long>
        {
            { 1, 100 },
            { 2, 200 },
            { 3, 300 }
        };

        var localUpdates = new Dictionary<int, long>
        {
            { 1, 100 },
            { 2, 150 }
        };

        _tvMazeScraperServiceMock.Setup(s => s.GetTVShowUpdatesAsync()).ReturnsAsync(remoteUpdates);
        _tvShowRepositoryMock.Setup(r => r.GetLastUpdatedMoment()).ReturnsAsync(localUpdates);

        // Act
        await _useCase.ExecuteAsync();

        // Assert
        _loggerMock.VerifyLogInfo("Identified 1 new TVShows.", Times.Once);
        _loggerMock.VerifyLogInfo("Identified 1 outdated TVShows.", Times.Once);
        _loggerMock.VerifyLogInfo("Enqueuing 2 TVShows for update.", Times.Once);
        
        _queueMock.Verify(q => q.EnqueueMany(It.Is<List<TVShowId>>(list =>
                list.Count == 2 && list.Contains(new TVShowId(2)) && list.Contains(new TVShowId(3))
        )), Times.Once);
    }
    
    [Fact]
    public async Task GivenAnExceptionThrown_WhenGettingToBeUpserted_ThenLogsErrorAndThrowsException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        _tvMazeScraperServiceMock.Setup(s => s.GetTVShowUpdatesAsync()).ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync());

        _loggerMock.VerifyLogError("An error occurred while updating missing TVShows.", exception, Times.Once);
        Assert.Equal("Test exception", ex.Message);
    }
    
    [Fact]
    public async Task GivenEmptyTVShows_WhenGettingToBeUpserted_ThenLogsCorrectMessagesAndCompletes()
    {
        // Given
        var remoteUpdates = new Dictionary<int, long>();
        var localUpdates = new Dictionary<int, long>();

        _tvMazeScraperServiceMock.Setup(s => s.GetTVShowUpdatesAsync()).ReturnsAsync(remoteUpdates);
        _tvShowRepositoryMock.Setup(r => r.GetLastUpdatedMoment()).ReturnsAsync(localUpdates);

        // When
        await _useCase.ExecuteAsync();

        // Then
        _loggerMock.VerifyLogInfo("Identified 0 new TVShows.", Times.Once);
        _loggerMock.VerifyLogInfo("Identified 0 outdated TVShows.", Times.Once);
        _loggerMock.VerifyLogInfo("Enqueuing 0 TVShows for update.", Times.Once);

        _queueMock.Verify(q => q.EnqueueMany(It.IsAny<List<TVShowId>>()), Times.Never());
    }
    
    [Fact]
    public async Task GivenNoChangesBetweenRemoteAndLocal_WhenGettingToBeUpserted_ThenLogsCorrectlyAndCompletes()
    {
        // Given
        var remoteUpdates = new Dictionary<int, long>
        {
            { 1, 100 },
            { 2, 200 }
        };
        var localUpdates = new Dictionary<int, long>
        {
            { 1, 100 },
            { 2, 200 }
        };

        _tvMazeScraperServiceMock.Setup(s => s.GetTVShowUpdatesAsync()).ReturnsAsync(remoteUpdates);
        _tvShowRepositoryMock.Setup(r => r.GetLastUpdatedMoment()).ReturnsAsync(localUpdates);

        // When
        await _useCase.ExecuteAsync();

        // Then
        _loggerMock.VerifyLogInfo("Identified 0 new TVShows.", Times.Once);
        _loggerMock.VerifyLogInfo("Identified 0 outdated TVShows.", Times.Once);
        _loggerMock.VerifyLogInfo("Enqueuing 0 TVShows for update.", Times.Once);
        
        _queueMock.Verify(q => q.EnqueueMany(It.IsAny<List<TVShowId>>()), Times.Never());
    }
}