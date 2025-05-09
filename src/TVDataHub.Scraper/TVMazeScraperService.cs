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
    
    public async Task<IReadOnlyList<TVMazeShowDto>> GetPaginatedTVShowAsync(int page)
    {
        var requestUri = $"{_settings.TVShowsPaginatedApi}{page}";
        var response = await httpClient.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<TVMazeShowDto>();
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await DeserializeAsync<IReadOnlyList<TVMazeShowDto>>(stream) ?? Array.Empty<TVMazeShowDto>();
    }

    public async Task<TVMazeShowWithCastDto?> GetTVShowAsync(int id)
    {
        var requestUri = _settings.TVShowByIdWithEmbedCastApi.Replace("{id}", id.ToString());

        var response = await httpClient.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TVMazeShowWithCastDto>(content);
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

    public async Task<Dictionary<int, long>> GetTVShowDailyUpdatesAsync()
    {
        var response = await httpClient.GetAsync($"{_settings.TVShowsUpdatesApi}?since=day");
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