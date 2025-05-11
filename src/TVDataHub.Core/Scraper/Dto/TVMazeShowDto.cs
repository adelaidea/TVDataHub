using System.Text.Json.Serialization;

namespace TVDataHub.Core.Scraper.Dto;

public class TVMazeShowDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<string> Genres { get; set; } = new();

    public DateOnly? Premiered { get; set; }

    public DateOnly? Ended { get; set; }

    public long Updated { get; set; }

    [JsonPropertyName("_embedded")] 
    public EmbeddedDto Embedded { get; set; } = new();

    [JsonIgnore]
    public List<TVMazeCastDto> Cast =>
        Embedded.Cast.DistinctBy(c => c.Person.Id).ToList();
}