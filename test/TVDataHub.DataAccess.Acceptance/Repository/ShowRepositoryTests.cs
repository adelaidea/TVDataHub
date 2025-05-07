using FluentAssertions;
using TVDataHub.DataAccess.Repository;
using TVDataHub.Domain.Entity;

namespace TVDataHub.DataAccess.Acceptance.Repository;

public class
    ShowRepositoryTests(TestBase fixture) : IClassFixture<TestBase>
{
    private readonly CastMemberRepository _castMemberRepository = new(fixture.DbContext);
    private readonly ShowRepository _showRepository = new(fixture.DbContext);

    [Fact]
    public async Task GivenANonExistingShow_WhenUpserting_NewValueShouldBeCreated()
    {
        // Arrange
        var show = new Show
        {
            Id = 100,
            Name = "New Show"
        };

        // Act
        await _showRepository.UpsertShow(show);

        // Assert
        var result = await fixture.DbContext.Shows.FindAsync(show.Id);
        
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Show");
    }
    
    [Fact]
    public async Task GivenAnExistingShow_WhenUpserting_ExistingValueShouldBeUpdated()
    {
        // Arrange
        var show = new Show
        {
            Id = 200,
            Name = "Initial Name"
        };
        await _showRepository.UpsertShow(show);

        var updatedShow = new Show
        {
            Id = 200,
            Name = "Updated Name"
        };

        // Act
        await _showRepository.UpsertShow(updatedShow);

        // Assert
        var result = await fixture.DbContext.Shows.FindAsync(updatedShow.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
    }
    
    [Fact]
    public async Task GivenAStoredShowWithCast_WhenGettingPaginated_ShouldReturnShowWithOrderedCast()
    {
        // Arrange
        var show = new Show
        {
            Id = 300,
            Name = "Show With Cast",
        };

        var castMembers = new List<CastMember>
        {
            new() { Id = 100, Name = "Cast 1", Birthday = new DateOnly(2000, 7, 10), ShowId = show.Id },
            new() { Id = 200, Name = "Cast 2", Birthday = new DateOnly(1985, 3, 20), ShowId = show.Id },
            new() { Id = 300, Name = "Cast 3", Birthday = new DateOnly(1995, 10, 1), ShowId = show.Id },
        };

        await _showRepository.UpsertShow(show);
        await _castMemberRepository.UpsertCastMembers(castMembers);

        // Act
        var result = await _showRepository.GetPaginated();

        var test = result.First().Cast.OrderBy(x => x.Birthday);
        
        // Assert
        Assert.Single(result);

        var returnedShow = result.First();
        Assert.Equal(show.Id, returnedShow.Id);
        Assert.Equal(show.Name, returnedShow.Name);
        Assert.Equal(3, returnedShow.Cast.Count);
        
        returnedShow.Cast
            .Select(c => c.Id)
            .Should()
            .Contain([200,300,100]);
    }
}