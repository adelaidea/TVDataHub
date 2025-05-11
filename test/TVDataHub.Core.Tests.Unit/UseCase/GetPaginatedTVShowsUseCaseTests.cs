using Microsoft.Extensions.Logging;
using Moq;
using TVDataHub.Core.Domain.Entity;
using TVDataHub.Core.Dto;
using TVDataHub.Core.Repository;
using TVDataHub.Core.Tests.Unit.Extensions;
using TVDataHub.Core.UseCase;

namespace TVDataHub.Core.Tests.Unit.UseCase;

public class GetPaginatedTVShowsUseCaseTests
{
    private readonly Mock<ITVShowRepository> _tvShowRepositoryMock;
    private readonly Mock<ILogger<GetPaginatedTVShowsUseCase>> _loggerMock;
    private readonly GetPaginatedTVShowsUseCase _useCase;
    private const int DefaultPageSize = 10;

    public GetPaginatedTVShowsUseCaseTests()
    {
        _tvShowRepositoryMock = new Mock<ITVShowRepository>();
        _loggerMock = new Mock<ILogger<GetPaginatedTVShowsUseCase>>();
        _useCase = new GetPaginatedTVShowsUseCase(
            _tvShowRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GivenMultipleShows_WhenGettingPaginated_ThenShouldReturnShowsAndOrdersCastDescending()
    {
        // Arrange
        var page = 1;
        var shows = new[]
        {
            BuildShow(
                id: 100,
                name: "Test Show A",
                genres: ["A", "B"],
                premiered: new DateOnly(2018,3,10),
                ended: new DateOnly(2020,5,20),
                cast:
                [
                    BuildPerson(1, "Young", new DateOnly(2000,1,1)),
                    BuildPerson(2, "Old", new DateOnly(1980,1,1))
                ]),
            BuildShow(
                id: 200,
                name: "Test Show B",
                genres: ["C"],
                premiered: new DateOnly(2019,7,1),
                ended: null,
                cast:
                [
                    BuildPerson(3, "Mid", new DateOnly(1990,6,15)),
                    BuildPerson(4, "Ancient", null),
                    BuildPerson(5, "New", new DateOnly(2005,9,5))
                ])
        };

        _tvShowRepositoryMock.Setup(r => r.GetPaginated(DefaultPageSize, page)).ReturnsAsync(shows);
        _tvShowRepositoryMock.Setup(r => r.GetTotal()).ReturnsAsync(100);

        // Act
        var result = await _useCase.ExecuteAsync(page);

        // Assert
        _tvShowRepositoryMock.Verify(r => r.GetPaginated(DefaultPageSize, page), Times.Once);
        _tvShowRepositoryMock.Verify(r => r.GetTotal(), Times.Once);

        Assert.Equal(100, result.TotalCount);
        
        Assert.Collection(result.Items,
            dto => AssertShowDto(shows[0], dto),
            dto => AssertShowDto(shows[1], dto)
        );
    }

    [Fact]
    public async Task GivenRepositoryThrowsAnException_WhenGettingPaginated_ThenShouldLogErrorMessageAnThrow()
    {
        // Arrange
        var error = new InvalidOperationException("fail");
        _tvShowRepositoryMock.Setup(r => r.GetPaginated(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(error);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApplicationException>(
            () => _useCase.ExecuteAsync(2));

        _loggerMock.VerifyLogError("Failed to get paginated TVShows.", error, Times.Once);
        Assert.Equal("Failed to retrieve TVShows. Please try again later.", ex.Message);
    }
    
    private static Person BuildPerson(int id, string name, DateOnly? birthday) =>
        new() { Id = id, Name = name, Birthday = birthday };
    
    private static TVShow BuildShow(
        int id, 
        string name, 
        string[] genres,
        DateOnly premiered, 
        DateOnly? ended, 
        Person[] cast
        ) =>
        new()
        {
            Id = id,
            Name = name,
            Genres = genres.ToList(),
            Premiered = premiered,
            Ended = ended,
            Cast = cast.ToList()
        };
    
    private static void AssertShowDto(TVShow expected, TVShowDto actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Genres, actual.Genres);
        Assert.Equal(expected.Premiered, actual.Premiered);
        Assert.Equal(expected.Ended, actual.Ended);

        var expectedOrdered = expected.Cast
            .OrderByDescending(p => p.Birthday)
            .Select(p => p.Id)
            .ToList();
        
        var actualIds = actual.Cast.Select(c => c.Id).ToList();
        Assert.Equal(expectedOrdered, actualIds);
    }
}