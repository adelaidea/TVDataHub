namespace TVDataHub.Core.UseCase.Response;

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; }
    
    public int TotalCount { get; init; }
}