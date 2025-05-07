namespace TVDataHub.Domain.Entity;

public class CastMember
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateOnly Birthday { get; set; }

    public int ShowId { get; set; }

    public Show Show { get; set; }
}
