namespace TVDataHub.Scraper.Settings;

internal sealed class TVMazeSettings
{
    public string BaseApi { get; init; }
    
    public string TVShowsPaginatedApi { get; init; }

    public string TVShowByIdWithEmbedCastApi { get; init; }
    
    public string TVShowsUpdatesApi { get; init; }
    
    public string TVShowCastApi { get; init; }
}