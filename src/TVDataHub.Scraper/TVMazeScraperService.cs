using System.Text.Json;
using Microsoft.Extensions.Options;
using TVDataHub.Domain.Dto;
using TVDataHub.Domain.Scraper;
using TVDataHub.Scraper.Settings;

namespace TVDataHub.Scraper;

internal class TVMazeScraperService(
    HttpClient httpClient,
    IOptions<TVMazeSettings> settings) : ITVMazeScraperService
{
    private readonly TVMazeSettings _settings = settings.Value;
    
    public async Task<IReadOnlyList<TVMazeTVShowDto>> GetPaginatedTVShow(int page)
    {
        using var response = await httpClient.GetAsync($"{_settings.TVShowsApi}{page}");

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<TVMazeTVShowDto>();
        }

        await using var content = await response.Content.ReadAsStreamAsync();

        var tvShows = await JsonSerializer.DeserializeAsync<IReadOnlyList<TVMazeTVShowDto>>(content) ?? Array.Empty<TVMazeTVShowDto>();

        return tvShows;
    }

    public async Task<TVMazeTVShowDto?> GetTVShow(int id)
    {
        var requestUri = _settings.TVShowById.Replace("{id}", id.ToString());
        
        using var response = await httpClient.GetAsync(requestUri);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var TVshow = JsonSerializer.Deserialize<TVMazeTVShowDto>(content);

        return TVshow;
    }

    public async Task<IReadOnlyList<TVMazeCastDto>> GetTVShowCastMembers(int tvShowId)
    {
        var requestUri = _settings.ShowCastApi.Replace($"{tvShowId}", tvShowId.ToString());

        using var response = await httpClient.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<TVMazeCastDto>();
        }

        await using var content = await response.Content.ReadAsStreamAsync();

        var castMembers = await JsonSerializer.DeserializeAsync<IReadOnlyList<TVMazeCastDto>>(content) ?? Array.Empty<TVMazeCastDto>();

        return castMembers;
    }
}