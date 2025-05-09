using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TVDataHub.DataAccess.Repository;
using TVDataHub.Domain.Entity;

namespace TVDataHub.DataAccess.Acceptance.Repository;

public class TVShowRepositoryTests(TestBase fixture) : IClassFixture<TestBase>
{
    private readonly CastMemberRepository _castMemberRepository = new(fixture.DbContext);
    private readonly TVShowRepository _tvShowRepository = new(fixture.DbContext);

    [Fact]
    public async Task GivenANonExistingTVShow_WhenUpserting_NewValueShouldBeCreated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 100,
            Name = "New TVShow"
        };

        // Act
        await _tvShowRepository.UpsertTVShow(tvShow);

        // Assert
        var result = await fixture.DbContext.TVShows.FindAsync(tvShow.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New TVShow");
    }

    [Fact]
    public async Task GivenANonExistingTVShow_WhenUpsertingWithCast_NewValueShouldBeCreated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 200,
            Name = "New TVShow",
            Cast = new List<CastMember>
            {
                new() { Id = 100, Name = "Cast 1", Birthday = new DateOnly(2000, 7, 10) },
                new() { Id = 200, Name = "Cast 2", Birthday = new DateOnly(1985, 3, 20) },
                new() { Id = 300, Name = "Cast 3", Birthday = new DateOnly(1995, 10, 1) },
            }
        };

        // Act
        await _tvShowRepository.UpsertTVShowWithCast(tvShow);

        // Assert
        var result = await fixture.DbContext.TVShows
            .Include(s => s.Cast)
            .SingleOrDefaultAsync(s => s.Id == tvShow.Id);

        result.Should().NotBeNull();
        result!.Cast.Should().HaveCount(3);
        result!.Name.Should().Be("New TVShow");
    }

    [Fact]
    public async Task GivenAnExistingTVShow_WhenUpserting_ExistingValueShouldBeUpdated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 300,
            Name = "Initial Name"
        };
        await _tvShowRepository.UpsertTVShow(tvShow);

        var updatedTVShow = new TVShow
        {
            Id = 300,
            Name = "Updated Name"
        };

        // Act
        await _tvShowRepository.UpsertTVShow(updatedTVShow);

        // Assert
        var result = await fixture.DbContext.TVShows.FindAsync(updatedTVShow.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task GivenAStoredTVShowWithCast_WhenGettingPaginated_ShouldReturnTVShowWithCast()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 400,
            Name = "TVShow With Cast",
        };

        var castMembers = new List<CastMember>
        {
            new() { Id = 400, Name = "Cast 1", Birthday = new DateOnly(2000, 7, 10), TVShowId = tvShow.Id },
            new() { Id = 500, Name = "Cast 2", Birthday = new DateOnly(1985, 3, 20), TVShowId = tvShow.Id },
            new() { Id = 600, Name = "Cast 3", Birthday = new DateOnly(1995, 10, 1), TVShowId = tvShow.Id },
        };

        await _tvShowRepository.UpsertTVShow(tvShow);
        await _castMemberRepository.UpsertCastMembers(castMembers);

        // Act
        var result = await _tvShowRepository.GetPaginated(1);

        // Assert
        Assert.Single(result);

        var savedTVShow = result.First();
        Assert.Equal(tvShow.Id, savedTVShow.Id);
        Assert.Equal(tvShow.Name, savedTVShow.Name);
        Assert.Equal(3, savedTVShow.Cast.Count);

        savedTVShow.Cast
            .Select(c => c.Id)
            .Should()
            .Contain([200, 300, 100]);
    }

    [Fact]
    public async Task GivenTheTVShowTableWithValue_WhenGettingLastId_ShouldReturnTheCorrectValue()
    {
        // Arrange
        var tvShows = new List<TVShow>
        {
            new() { Id = 500, Name = "TVShow With Cast" },
            new() { Id = 600, Name = "TVShow With Cast" },
            new() { Id = 700, Name = "TVShow With Cast" },
        };

        await _tvShowRepository.UpsertTVShows(tvShows);

        // Act
        var result = await _tvShowRepository.GetLastId();

        // Assert
        result.Should().Be(700);
    }
}