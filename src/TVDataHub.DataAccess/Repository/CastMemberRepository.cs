using Microsoft.EntityFrameworkCore;
using TVDataHub.Core.Domain.Entity;
using TVDataHub.Core.Repository;

namespace TVDataHub.DataAccess.Repository;

public class CastMemberRepository(
    TVDataHubContext dbContext) : ICastMemberRepository
{
    public async Task UpsertCastMember(CastMember castMember)
    {
        var existing = await dbContext.CastMembers
            .FirstOrDefaultAsync(c => c.Id == castMember.Id);

        if (existing == null)
        {
            await dbContext.CastMembers.AddAsync(castMember);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(castMember);
        }

        await dbContext.SaveChangesAsync();
    }
    
    public async Task UpsertCastMembers(IEnumerable<CastMember> castMembers)
    {
        var castMemberList = castMembers.ToList();
        var ids = castMemberList.Select(c => c.Id).ToList();

        var existingCastMembers = await dbContext.CastMembers
            .Where(c => ids.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

        foreach (var castMember in castMemberList)
        {
            if (existingCastMembers.TryGetValue(castMember.Id, out var existing))
            {
                dbContext.Entry(existing).CurrentValues.SetValues(castMember);
            }
            else
            {
                await dbContext.CastMembers.AddAsync(castMember);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}