namespace TVDataHub.Core.Domain.Entity;

public class CastMember
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateOnly Birthday { get; set; }

    public int TVShowId { get; set; }

    public TVShow TVShow { get; set; }
}