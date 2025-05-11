using FluentAssertions;
using TVDataHub.Core.Domain.Entity;
using TVDataHub.DataAccess.Repository;

namespace TVDataHub.DataAccess.Acceptance.Repository;

public class
    CastMemberRepositoryTests(TestBase fixture) : IClassFixture<TestBase>
{
    private readonly CastMemberRepository _castMemberRepository = new(fixture.DbContext);
    private readonly TVShowRepository _tvShowRepository = new(fixture.DbContext);

    [Fact]
    public async Task GivenANonExistedCastMember_WhenUpserting_NewValueShouldBeCreated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 1,
            Name = "Test TVShow"
        };

        var birthday = new DateOnly(1990, 5, 10);

        var castMember = new CastMember
        {
            Id = 1,
            Name = "Test Actor",
            Birthday = birthday,
            TVShowId = tvShow.Id
        };

        await _tvShowRepository.UpsertTVShow(tvShow);

        // Act
        await _castMemberRepository.UpsertCastMember(castMember);

        // Assert
        var value = await fixture.DbContext.CastMembers.FindAsync(castMember.Id);
        
        value.Should().NotBeNull();
        value!.Name.Should().Be("Test Actor");
        value.Birthday.Should().Be(new DateOnly(1990, 5, 10));
        value.TVShowId.Should().Be(tvShow.Id);
    }

    [Fact]
    public async Task GivenAnExistingCastMember_WhenUpserting_ExistingValueShouldBeUpdated()
    {
        // Arrange
        var tvShow = new TVShow
        {
            Id = 2,
            Name = "Test TVShow 1"
        };

        await _tvShowRepository.UpsertTVShow(tvShow);

        var originalCast = new CastMember
        {
            Id = 2,
            Name = "Initial Name",
            Birthday = new DateOnly(1990, 1, 1),
            TVShowId = tvShow.Id
        };

        await _castMemberRepository.UpsertCastMember(originalCast);

        var updatedCast = new CastMember
        {
            Id = 2,
            Name = "Updated Name",
            Birthday = new DateOnly(1985, 12, 25),
            TVShowId = tvShow.Id
        };

        // Act
        await _castMemberRepository.UpsertCastMember(updatedCast);

        // Assert
        var value = await fixture.DbContext.CastMembers.FindAsync(updatedCast.Id);

        value.Should().NotBeNull();
        value!.Name.Should().Be("Updated Name");
        value.Birthday.Should().Be(new DateOnly(1985, 12, 25));
        value.TVShowId.Should().Be(tvShow.Id);
    }
}