namespace TVDataHub.Core.Dto;

public record CastMemberDto(
    int Id, 
    string Name, 
    DateOnly? Birthday,
    DateOnly? Deathday,
    string Gender);