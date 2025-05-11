using TVDataHub.Core.Domain.Entity;

namespace TVDataHub.Core.Repository;

public interface ICastMemberRepository
{
    Task UpsertCastMember(CastMember castMember);

    Task UpsertCastMembers(IEnumerable<CastMember> castMembers);
}