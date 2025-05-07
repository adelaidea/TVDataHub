using TVDataHub.Domain.Entity;

namespace TVDataHub.Domain.Repository;

public interface ICastMemberRepository
{
    Task UpsertCastMember(CastMember castMember);

    Task UpsertCastMembers(IEnumerable<CastMember> castMembers);
}