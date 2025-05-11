namespace TVDataHub.Core.Dto;

public record TVShowDto(
    int Id,
    string Name,
    List<string> Genres,
    DateOnly? Premiered,
    DateOnly? Ended,
    IReadOnlyList<CastMemberDto> Cast);