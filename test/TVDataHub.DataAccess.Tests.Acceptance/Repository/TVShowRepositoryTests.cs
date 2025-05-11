using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TVDataHub.Core.Domain.Entity;
using TVDataHub.DataAccess.Repository;

namespace TVDataHub.DataAccess.Tests.Acceptance.Repository;

public class TVShowRepositoryTests(TestBase fixture) : IClassFixture<TestBase>
{
    private readonly TVShowRepository _tvShowRepository = new(fixture.DbContext);

    [Fact]
    public async Task GivenANonExistingTVShow_WhenUpserting_NewValueShouldBeCreated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 100,
            Name = "New TVShow",
            Genres = ["Drama", "Mystery"],
            Premiered = new DateOnly(2023, 1, 15),
            Ended = new DateOnly(2024, 1, 15),
            Updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        // Act
        await _tvShowRepository.UpsertTVShow(tvShow);

        // Assert
        var result = await fixture.DbContext.TVShows.FindAsync(tvShow.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(tvShow.Id);
        result.Name.Should().Be(tvShow.Name);
        result.Genres.Should().BeEquivalentTo(tvShow.Genres);
        result.Premiered.Should().Be(tvShow.Premiered);
        result.Ended.Should().Be(tvShow.Ended);
        result.Updated.Should().Be(tvShow.Updated);
    }

    [Fact]
    public async Task GivenAnExistingTVShow_WhenUpserting_ExistingValueShouldBeUpdated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 200,
            Name = "Initial Name",
            Genres = ["Comedy"],
            Premiered = new DateOnly(2020, 5, 1),
            Ended = new DateOnly(2022, 5, 1),
            Updated = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds(),
        };

        await _tvShowRepository.UpsertTVShow(tvShow);

        var updatedTVShow = new TVShow
        {
            Id = 200,
            Name = "Updated Name",
            Genres = ["Drama", "Thriller"],
            Premiered = new DateOnly(2021, 1, 1),
            Ended = new DateOnly(2023, 1, 1),
            Updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        // Act
        await _tvShowRepository.UpsertTVShow(updatedTVShow);

        // Assert
        var result = await fixture.DbContext.TVShows.FindAsync(updatedTVShow.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be(updatedTVShow.Name);
        result.Genres.Should().BeEquivalentTo(updatedTVShow.Genres);
        result.Premiered.Should().Be(updatedTVShow.Premiered);
        result.Ended.Should().Be(updatedTVShow.Ended);
        result.Updated.Should().Be(updatedTVShow.Updated);
    }

    [Fact]
    public async Task GivenANonExistingTVShow_WhenUpsertingWithCastAndCastDoesNotExists_NewValueShouldBeCreated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 300,
            Name = "Test TVShow",
            Genres = ["Drama", "Crime"],
            Premiered = new DateOnly(2025, 2, 8),
            Updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Cast = new List<Person>
            {
                new() { Id = 100, Name = "Cast 1", Birthday = new DateOnly(2000, 7, 10), Gender = "Male" },
                new() { Id = 200, Name = "Cast 2", Birthday = new DateOnly(1985, 3, 20), Gender = "Male" },
                new() { Id = 300, Name = "Cast 3", Birthday = new DateOnly(1995, 10, 1), Gender = "Female" },
            }
        };

        // Act
        await _tvShowRepository.UpsertTVShowWithCast(tvShow);

        // Assert
        var result = await fixture.DbContext.TVShows
            .Include(s => s.Cast)
            .SingleOrDefaultAsync(s => s.Id == tvShow.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(tvShow.Id);
        result.Name.Should().Be(tvShow.Name);
        result.Genres.Should().BeEquivalentTo(tvShow.Genres);
        result.Premiered.Should().Be(tvShow.Premiered);
        result.Ended.Should().BeNull();
        result.Updated.Should().Be(tvShow.Updated);

        result.Cast.Should().HaveCount(3);

        result.Cast.OrderBy(c => c.Id)
            .Should()
            .BeEquivalentTo(tvShow.Cast.OrderBy(c => c.Id), options =>
                options.Excluding(c => c.TVShows)
                    .WithStrictOrdering());
    }

    [Fact]
    public async Task GivenAnExistingTVShow_WhenUpsertingWithCastAndCastDataAlsoChanges_ValuesShouldBeUpdated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 400,
            Name = "New TVShow",
            Genres = ["Sci-Fi", "Horror"],
            Premiered = new DateOnly(2016, 7, 15),
            Ended = new DateOnly(2020, 10, 10),
            Updated = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
            Cast = new List<Person>
            {
                new() { Id = 400, Name = "Cast 1", Birthday = new DateOnly(2000, 7, 10), Gender = "Male" },
                new()
                {
                    Id = 500, Name = "Cast 2", Birthday = new DateOnly(1985, 3, 20),
                    Deathday = new DateOnly(2025, 05, 05)
                },
                new() { Id = 600, Name = "Cast 3", Birthday = new DateOnly(1995, 10, 1) },
            }
        };

        await _tvShowRepository.UpsertTVShowWithCast(tvShow);

        var updatedTVShow = new TVShow
        {
            Id = 400,
            Name = "Updated Name",
            Genres = ["Sci-Fi", "Mystery"],
            Premiered = tvShow.Premiered,
            Ended = new DateOnly(2021, 12, 1),
            Updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Cast = new List<Person>
            {
                new() { Id = 400, Name = "Cast 1 updated", Birthday = new DateOnly(2000, 7, 10) },
                new() { Id = 500, Name = "Cast 2", Birthday = new DateOnly(1985, 3, 20), Gender = "Female" },
                new()
                {
                    Id = 600, Name = "Cast 3", Birthday = new DateOnly(1995, 10, 1),
                    Deathday = new DateOnly(2025, 04, 29)
                },
                new() { Id = 700, Name = "Cast 4", Birthday = new DateOnly(2024, 5, 11) },
            }
        };

        // Act
        await _tvShowRepository.UpsertTVShowWithCast(updatedTVShow);

        // Assert
        var result = await fixture.DbContext.TVShows
            .Include(s => s.Cast)
            .SingleOrDefaultAsync(s => s.Id == tvShow.Id);

        result.Should().NotBeNull();

        result!.Name.Should().Be(updatedTVShow.Name);
        result.Genres.Should().BeEquivalentTo(updatedTVShow.Genres);
        result.Premiered.Should().Be(updatedTVShow.Premiered);
        result.Ended.Should().Be(updatedTVShow.Ended);
        result.Updated.Should().Be(updatedTVShow.Updated);

        result.Cast.Should().HaveCount(4);
        result.Cast.OrderBy(c => c.Id)
            .Should()
            .BeEquivalentTo(updatedTVShow.Cast.OrderBy(c => c.Id), options =>
                options.Excluding(c => c.TVShows)
                    .WithStrictOrdering());
    }

    [Fact]
    public async Task GivenAnExistingTVShow_WhenAddingAndRemovingFromCastList_ValuesShouldBeUpdated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 500,
            Name = "New TVShow",
            Genres = ["Sci-Fi", "Horror"],
            Updated = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
            Cast = new List<Person>
            {
                new() { Id = 800, Name = "Cast 1", Birthday = new DateOnly(2000, 7, 10) },
                new() { Id = 900, Name = "Cast 2", Birthday = new DateOnly(1985, 3, 20) },
                new() { Id = 1000, Name = "Cast 3", Birthday = new DateOnly(1995, 10, 1) },
            }
        };

        await _tvShowRepository.UpsertTVShowWithCast(tvShow);

        var updatedTVShow = new TVShow
        {
            Id = 500,
            Name = "New TVShow",
            Genres = ["Sci-Fi", "Horror"],
            Updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Cast = new List<Person>
            {
                new() { Id = 900, Name = "Cast 2", Birthday = new DateOnly(1985, 3, 20) },
                new() { Id = 1000, Name = "Cast 3 updated", Birthday = new DateOnly(1995, 10, 1) },
                new() { Id = 2000, Name = "Cast 4" },
            }
        };

        // Act
        await _tvShowRepository.UpsertTVShowWithCast(updatedTVShow);

        // Assert
        var result = await fixture.DbContext.TVShows
            .Include(s => s.Cast)
            .SingleOrDefaultAsync(s => s.Id == tvShow.Id);

        result.Should().NotBeNull();

        result!.Name.Should().Be(updatedTVShow.Name);
        result.Genres.Should().BeEquivalentTo(updatedTVShow.Genres);
        result.Premiered.Should().Be(updatedTVShow.Premiered);
        result.Ended.Should().Be(updatedTVShow.Ended);
        result.Updated.Should().Be(updatedTVShow.Updated);

        result.Cast.Should().HaveCount(3);
        result.Cast.OrderBy(c => c.Id)
            .Should()
            .BeEquivalentTo(updatedTVShow.Cast.OrderBy(c => c.Id), options =>
                options.Excluding(c => c.TVShows)
                    .WithStrictOrdering());
    }

    [Fact]
    public async Task GivenAStoredTVShowWithCast_WhenGettingPaginated_ShouldReturnTVShowWithCast()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 600,
            Name = "TVShow With Cast",
            Genres = ["Drama", "Thriller"],
            Premiered = new DateOnly(2025, 1, 1),
            Updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Cast = new List<Person>
            {
                new() { Id = 3000, Name = "Cast 1", Birthday = new DateOnly(2000, 7, 10) },
                new() { Id = 4000, Name = "Cast 2" },
                new() { Id = 5000, Name = "Cast 3", Birthday = new DateOnly(1995, 10, 1) },
            }
        };

        await _tvShowRepository.UpsertTVShowWithCast(tvShow);

        // Act
        var result = await _tvShowRepository.GetPaginated(10);

        // Assert
        var savedTVShow = result.First(s => s.Id == tvShow.Id);

        savedTVShow.Id.Should().Be(tvShow.Id);
        savedTVShow.Name.Should().Be(tvShow.Name);
        savedTVShow.Genres.Should().BeEquivalentTo(tvShow.Genres);
        savedTVShow.Premiered.Should().Be(tvShow.Premiered);
        savedTVShow.Ended.Should().Be(tvShow.Ended);
        savedTVShow.Updated.Should().Be(tvShow.Updated);
        savedTVShow.Cast.Should().HaveCount(3);

        savedTVShow.Cast.Select(c => c.Id)
            .Should()
            .BeEquivalentTo([3000, 4000, 5000]);
    }

    [Fact]
    public async Task GivenMoreThan10StoredTVShowWithCast_WhenGettingPaginated_ShouldReturnOnly10TVShows()
    {
        // Arrange
        await fixture.DbContext.TVShows.ExecuteDeleteAsync();
        await fixture.DbContext.Persons.ExecuteDeleteAsync();
        await fixture.DbContext.SaveChangesAsync();

        var tvShows = new List<TVShow>();

        for (int i = 0; i < 15; i++)
        {
            tvShows.Add(new TVShow
            {
                Id = 700 + i,
                Name = "New TVShow",
                Genres = ["Drama", "Mystery"],
                Premiered = new DateOnly(2023, 1, 15),
                Ended = new DateOnly(2024, 1, 15),
                Updated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            });
        }

        // Act
        await _tvShowRepository.UpsertTVShows(tvShows);

        // Act
        var result = await _tvShowRepository.GetPaginated(10);

        // Assert
        result.Should().HaveCount(10);
        result.Select(s => s.Id)
            .Should()
            .BeEquivalentTo([700, 701, 702, 703, 704, 705, 706, 707, 708, 709]);
    }
}