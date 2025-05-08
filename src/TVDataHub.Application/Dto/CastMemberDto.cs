namespace TVDataHub.Application.Dto;

public record CastMemberDto(
    int Id, 
    string Name, 
    DateOnly? Birthday);
