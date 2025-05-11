namespace TVDataHub.Core.Scraper.Dto;

public class EmbeddedDto
{
    public List<TVMazeCastDto> Cast { get; set; } = new();
}

public class TVMazeCastDto
{
    public TVMazePersonDto Person { get; set; }
}

public class TVMazePersonDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = String.Empty;
    
    public DateOnly? Birthday { get; set; }

    public DateOnly? Deathday { get; set; }

    public string Gender { get; set; } = string.Empty;
}