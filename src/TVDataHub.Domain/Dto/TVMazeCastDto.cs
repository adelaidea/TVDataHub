namespace TVDataHub.Domain.Dto;

public sealed class TVMazeCastDto
{
    public TVMazePersonDto Person { get; set; }
}

public sealed class TVMazePersonDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = String.Empty;
    
    public DateOnly Birthday { get; set; }

    public DateOnly? Deathday { get; set; }

    public string Gender { get; set; } = string.Empty;
}