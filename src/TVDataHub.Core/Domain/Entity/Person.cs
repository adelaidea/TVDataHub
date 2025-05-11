namespace TVDataHub.Core.Domain.Entity;

public class Person
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly? Birthday { get; set; }

    public ICollection<TVShow> TVShows { get; set; } = new List<TVShow>();
}