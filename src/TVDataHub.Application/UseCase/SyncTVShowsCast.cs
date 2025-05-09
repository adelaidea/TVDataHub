namespace TVDataHub.Application.UseCase;

public interface IGetAndInsertTVShowsCast
{
    Task ExecuteAsync();
}

internal sealed class SyncTVShowsCast : IGetAndInsertTVShowsCast
{
    public Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}