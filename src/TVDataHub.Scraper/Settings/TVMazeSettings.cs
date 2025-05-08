namespace TVDataHub.Scraper.Settings;

internal sealed class TVMazeSettings
{
    public string BaseApi { get; init; }
    
    public string TVShowsApi { get; init; }

    public string TVShowById { get; init; }
    
    public string TVShowsUpdatesApi { get; init; }
    
    public string ShowCastApi { get; init; }
}