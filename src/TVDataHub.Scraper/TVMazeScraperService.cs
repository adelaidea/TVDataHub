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
    
    public async Task<IReadOnlyList<TVMazeTVShowDto>> GetPaginatedTVShowAsync(int page)
    {
        var requestUri = $"{_settings.TVShowsPaginatedApi}{page}";
        var response = await httpClient.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<TVMazeTVShowDto>();
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await DeserializeAsync<IReadOnlyList<TVMazeTVShowDto>>(stream) ?? Array.Empty<TVMazeTVShowDto>();
    }

    public async Task<TVMazeTVShowDto?> GetTVShowAsync(int id)
    {
        var requestUri = _settings.TVShowByIdApi.Replace("{id}", id.ToString());

        var response = await httpClient.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TVMazeTVShowDto>(content);
    }

    public async Task<IReadOnlyList<TVMazeCastDto>> GetTVShowCastMembersAsync(int tvShowId)
    {
        var requestUri = _settings.TVShowCastApi.Replace("{id}", tvShowId.ToString());

        var response = await httpClient.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<TVMazeCastDto>();
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await DeserializeAsync<IReadOnlyList<TVMazeCastDto>>(stream) ?? Array.Empty<TVMazeCastDto>();
    }

    public async Task<Dictionary<int, long>> GetShowUpdatesAsync()
    {
        var response = await httpClient.GetAsync(_settings.TVShowsUpdatesApi);
        if (!response.IsSuccessStatusCode)
        {
            return new Dictionary<int, long>();
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await DeserializeAsync<Dictionary<int, long>>(stream) ?? new Dictionary<int, long>();
    }

    private static async Task<T?> DeserializeAsync<T>(Stream stream)
    {
        return await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}