namespace TVDataHub.Application.UseCase;

public interface IGetAndUpdateTVShowsUseCase
{
    Task ExecuteAsync();
}

internal sealed class SyncUpdatedTVShowsUseCase : IGetAndUpdateTVShowsUseCase
{
    public Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}