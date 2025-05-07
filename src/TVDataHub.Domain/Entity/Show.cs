namespace TVDataHub.Domain.Entity;

public class Show
{
    public int Id { get; init; }

    public string Name { get; init; }

    public ICollection<CastMember> Cast { get; init; }
}