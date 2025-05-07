using TVDataHub.Domain.Entity;

namespace TVDataHub.Domain.Repository;

public interface IShowRepository
{
    Task UpsertShow(Show show);

    Task<IReadOnlyCollection<Show>> GetPaginated(int page = 1);
}