namespace TVDataHub.Domain.Entity;

public class TVShow
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public List<string> Genres { get; set; } = new();
    
    public DateOnly? Premiered { get; set; }
    
    public DateOnly? Ended { get; set; }
    
    public long Updated { get; set; }

    public ICollection<CastMember> Cast { get; init; }
}