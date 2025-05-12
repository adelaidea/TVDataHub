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
        var tvShow = CreateTVShow(
            100,
            "New TVShow",
            ["Drama", "Mystery"],
            new DateOnly(2023, 1, 15),
            new DateOnly(2024, 1, 15),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            null);

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
        var original = CreateTVShow(
            200,
            "Initial Name",
            ["Comedy"],
            new DateOnly(2020, 5, 1),
            new DateOnly(2022, 5, 1),
            DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds(),
            null);

        await _tvShowRepository.UpsertTVShow(original);

        var updated = CreateTVShow(
            200,
            "Updated Name",
            ["Drama", "Thriller"],
            new DateOnly(2021, 1, 1),
            new DateOnly(2023, 1, 1),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            null);

        // Act
        await _tvShowRepository.UpsertTVShow(updated);

        // Assert
        var result = await fixture.DbContext.TVShows.FindAsync(updated.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be(updated.Name);
        result.Genres.Should().BeEquivalentTo(updated.Genres);
        result.Premiered.Should().Be(updated.Premiered);
        result.Ended.Should().Be(updated.Ended);
        result.Updated.Should().Be(updated.Updated);
    }

    [Fact]
    public async Task GivenANonExistingTVShow_WhenUpsertingWithCastAndCastDoesNotExists_NewValueShouldBeCreated()
    {
        // Arrange
        var cast = new List<Person>
        {
            CreatePerson(100, "Cast 1", new DateOnly(2000, 7, 10), "Male", null),
            CreatePerson(200, "Cast 2", new DateOnly(1985, 3, 20), "Male", new DateOnly(2025, 5, 5)),
            CreatePerson(300, "Cast 3", new DateOnly(1995, 10, 1), "Female", null)
        };

        var tvShow = CreateTVShow(
            300,
            "Test TVShow",
            ["Drama", "Crime"],
            new DateOnly(2021, 1, 1),
            null,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            cast: cast);

        // Act
        await _tvShowRepository.UpsertTVShowWithCast(tvShow);

        // Assert
        var result = await fixture.DbContext.TVShows.Include(s => s.Cast).SingleOrDefaultAsync(s => s.Id == tvShow.Id);

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
                options.Excluding(c => c.TVShows).WithStrictOrdering());
    }

    [Fact]
    public async Task GivenAnExistingTVShow_WhenUpsertingWithCastAndCastDataAlsoChanges_ValuesShouldBeUpdated()
    {
        // Arrange
        var initialCast = new List<Person>
        {
            CreatePerson(400, "Cast 1", new DateOnly(2000, 7, 10), null, null),
            CreatePerson(500, "Cast 2", new DateOnly(1985, 3, 20), null, new DateOnly(2025, 5, 5)),
            CreatePerson(600, "Cast 3", new DateOnly(1995, 10, 1), "Male", new DateOnly(2024, 12, 20))
        };

        var initial = CreateTVShow(
            400,
            "New TVShow",
            ["Sci-Fi", "Horror"],
            new DateOnly(2016, 7, 15),
            new DateOnly(2020, 10, 10),
            DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
            initialCast);

        await _tvShowRepository.UpsertTVShowWithCast(initial);

        var updatedCast = new List<Person>
        {
            CreatePerson(400, "Cast 1 updated", new DateOnly(2000, 7, 10), null, new DateOnly(2025, 2, 6)),
            CreatePerson(500, "Cast 2", new DateOnly(1985, 3, 20), "Female", null),
            CreatePerson(600, "Cast 3", new DateOnly(1995, 10, 1), null, new DateOnly(2025, 4, 29)),
            CreatePerson(700, "Cast 4", new DateOnly(2024, 5, 11), "Male", null)
        };

        var updated = CreateTVShow(
            400,
            "Updated Name",
            ["Sci-Fi", "Mystery"],
            new DateOnly(2016, 7, 15),
            new DateOnly(2021, 12, 1),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            cast: updatedCast);

        // Act
        await _tvShowRepository.UpsertTVShowWithCast(updated);

        // Assert
        var result = await fixture.DbContext.TVShows.Include(s => s.Cast).SingleOrDefaultAsync(s => s.Id == updated.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be(updated.Name);
        result.Genres.Should().BeEquivalentTo(updated.Genres);
        result.Premiered.Should().Be(updated.Premiered);
        result.Ended.Should().Be(updated.Ended);
        result.Updated.Should().Be(updated.Updated);
        result.Cast.Should().HaveCount(4);

        result.Cast.OrderBy(c => c.Id)
            .Should()
            .BeEquivalentTo(updated.Cast.OrderBy(c => c.Id), options =>
                options.Excluding(c => c.TVShows).WithStrictOrdering());
    }

    [Fact]
    public async Task GivenAnExistingTVShow_WhenAddingAndRemovingFromCastList_ValuesShouldBeUpdated()
    {
        // Arrange
        var initialCast = new List<Person>
        {
            CreatePerson(800, "Cast 1", new DateOnly(2000, 7, 10), null, null),
            CreatePerson(900, "Cast 2", new DateOnly(1985, 3, 20), null, null),
            CreatePerson(1000, "Cast 3", null, null, null)
        };

        var initial = CreateTVShow(
            500,
            "New TVShow",
            ["Sci-Fi", "Horror"],
            new DateOnly(2016, 7, 15),
            new DateOnly(2021, 12, 1),
            DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
            cast: initialCast);

        await _tvShowRepository.UpsertTVShowWithCast(initial);

        var updatedCast = new List<Person>
        {
            CreatePerson(900, "Cast 2", new DateOnly(1985, 3, 20), "Male", null),
            CreatePerson(1000, "Cast 3 updated", new DateOnly(1995, 10, 1), null, new DateOnly(2025, 1, 1)),
            CreatePerson(2000, "Cast 4", null, null, null)
        };

        var updated = CreateTVShow(
            500,
            "New TVShow",
            ["Sci-Fi", "Horror"],
            new DateOnly(2016, 7, 15),
            new DateOnly(2021, 12, 1),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            cast: updatedCast);

        // Act
        await _tvShowRepository.UpsertTVShowWithCast(updated);

        // Assert
        var result = await fixture.DbContext.TVShows.Include(s => s.Cast).SingleOrDefaultAsync(s => s.Id == updated.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be(updated.Name);
        result.Genres.Should().BeEquivalentTo(updated.Genres);
        result.Premiered.Should().Be(updated.Premiered);
        result.Ended.Should().Be(updated.Ended);
        result.Updated.Should().Be(updated.Updated);
        result.Cast.Should().HaveCount(3);

        result.Cast.OrderBy(c => c.Id).Should()
            .BeEquivalentTo(updated.Cast.OrderBy(c => c.Id), options =>
                options.Excluding(c => c.TVShows).WithStrictOrdering());
    }

    [Fact]
    public async Task GivenAStoredTVShowWithCast_WhenGettingPaginated_ShouldReturnTVShowWithCast()
    {
        // Arrange
        var cast = new List<Person>
        {
            CreatePerson(3000, "Cast 1", new DateOnly(2000, 7, 10), null, null),
            CreatePerson(4000, "Cast 2", null, "Male", null),
            CreatePerson(5000, "Cast 3", new DateOnly(1995, 10, 1), null, new DateOnly(2024, 11, 10))
        };

        var tvShow = CreateTVShow(
            600,
            "TVShow With Cast",
            ["Drama", "Thriller"],
            new DateOnly(2016, 7, 15),
            new DateOnly(2021, 12, 1),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            cast: cast);

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
    public async Task GivenLessThan10StoredTVShow_WhenGettingPaginated_ShouldReturnAllTVShows()
    {
        // Arrange
        await fixture.DbContext.TVShows.ExecuteDeleteAsync();
        await fixture.DbContext.Persons.ExecuteDeleteAsync();
        await fixture.DbContext.SaveChangesAsync();

        var tvShows = Enumerable.Range(700, 5)
            .Select(i =>
                CreateTVShow(
                    i,
                    "New TVShow",
                    ["Drama", "Mystery"],
                    null,
                    null,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    null
                )).ToList();

        await _tvShowRepository.UpsertTVShows(tvShows);

        // Act
        var result = await _tvShowRepository.GetPaginated(10);

        // Assert
        result.Should().HaveCount(5);
        result.Select(s => s.Id).Should().BeEquivalentTo(Enumerable.Range(700, 5));
    }

    [Fact]
    public async Task GivenMoreThan10StoredTVShows_WhenGettingPaginated_ShouldReturnOnly10TVShows()
    {
        // Arrange
        await fixture.DbContext.TVShows.ExecuteDeleteAsync();
        await fixture.DbContext.Persons.ExecuteDeleteAsync();
        await fixture.DbContext.SaveChangesAsync();

        var tvShows = Enumerable.Range(800, 15)
            .Select(i =>
                CreateTVShow(
                    i,
                    "New TVShow",
                    ["Drama", "Mystery"],
                    null,
                    null,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    null
                )).ToList();

        await _tvShowRepository.UpsertTVShows(tvShows);

        // Act
        var result = await _tvShowRepository.GetPaginated(10);

        // Assert
        result.Should().HaveCount(10);
        result.Select(s => s.Id).Should().BeEquivalentTo(Enumerable.Range(800, 10));
    }

    private static TVShow CreateTVShow(
        int id,
        string name,
        List<string> genres,
        DateOnly? premiered,
        DateOnly? ended,
        long updated,
        List<Person>? cast) =>
        new()
        {
            Id = id,
            Name = name,
            Genres = genres,
            Premiered = premiered,
            Ended = ended,
            Updated = updated,
            Cast = cast
        };

    private static Person CreatePerson(
        int id,
        string name,
        DateOnly? birthday,
        string? gender,
        DateOnly? deathday) =>
        new()
        {
            Id = id,
            Name = name,
            Birthday = birthday,
            Gender = gender,
            Deathday = deathday
        };
}