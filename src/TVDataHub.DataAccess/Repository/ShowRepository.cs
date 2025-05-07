using Microsoft.EntityFrameworkCore;
using TVDataHub.Domain.Entity;
using TVDataHub.Domain.Repository;

namespace TVDataHub.DataAccess.Repository;

public class ShowRepository(
    TVDataHubContext dbContext) : IShowRepository
{
    private readonly int _pageSize = 10;
    
    public async Task UpsertShow(Show show)
    {
        var existingShow = await dbContext.Shows
            .FirstOrDefaultAsync(s => s.Id == show.Id);

        if (existingShow == null)
        {
            await dbContext.Shows.AddAsync(show);
        }
        else
        {            
            dbContext.Entry(existingShow).CurrentValues.SetValues(show);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Show>> GetPaginated(int page = 0) =>
        await dbContext.Shows
            .Include(s => s.Cast)
            .OrderBy(s => s.Id)
            .Skip(page * _pageSize)
            .Take(_pageSize)
            .ToListAsync();
}