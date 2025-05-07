using FluentAssertions;
using TVDataHub.DataAccess.Repository;
using TVDataHub.Domain.Entity;

namespace TVDataHub.DataAccess.Acceptance.Repository;

public class
    CastMemberRepositoryTests(TestBase fixture) : IClassFixture<TestBase>
{
    private readonly CastMemberRepository _castMemberRepository = new(fixture.DbContext);
    private readonly ShowRepository _showRepository = new(fixture.DbContext);

    [Fact]
    public async Task GivenANonExistedCastMember_WhenUpserting_NewValueShouldBeCreated()
    {
        // Arrange
        var show = new Show
        {
            Id = 1,
            Name = "Test Show"
        };

        var birthday = new DateOnly(1990, 5, 10);

        var castMember = new CastMember
        {
            Id = 1,
            Name = "Test Actor",
            Birthday = birthday,
            ShowId = show.Id
        };

        await _showRepository.UpsertShow(show);

        // Act
        await _castMemberRepository.UpsertCastMember(castMember);

        // Assert
        var value = await fixture.DbContext.CastMembers.FindAsync(castMember.Id);
        
        value.Should().NotBeNull();
        value!.Name.Should().Be("Test Actor");
        value.Birthday.Should().Be(new DateOnly(1990, 5, 10));
        value.ShowId.Should().Be(show.Id);
    }

    [Fact]
    public async Task GivenAnExistingCastMember_WhenUpserting_ExistingValueShouldBeUpdated()
    {
        // Arrange
        var show = new Show
        {
            Id = 2,
            Name = "Test Show 1"
        };

        await _showRepository.UpsertShow(show);

        var originalCast = new CastMember
        {
            Id = 2,
            Name = "Initial Name",
            Birthday = new DateOnly(1990, 1, 1),
            ShowId = show.Id
        };

        await _castMemberRepository.UpsertCastMember(originalCast);

        var updatedCast = new CastMember
        {
            Id = 2,
            Name = "Updated Name",
            Birthday = new DateOnly(1985, 12, 25),
            ShowId = show.Id
        };

        // Act
        await _castMemberRepository.UpsertCastMember(updatedCast);

        // Assert
        var value = await fixture.DbContext.CastMembers.FindAsync(updatedCast.Id);

        value.Should().NotBeNull();
        value!.Name.Should().Be("Updated Name");
        value.Birthday.Should().Be(new DateOnly(1985, 12, 25));
        value.ShowId.Should().Be(show.Id);
    }
}