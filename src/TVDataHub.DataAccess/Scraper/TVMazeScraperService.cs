using System.Text.Json;
using Microsoft.Extensions.Options;
using TVDataHub.Core.Scraper;
using TVDataHub.Core.Scraper.Dto;
using TVDataHub.DataAccess.Settings;

namespace TVDataHub.DataAccess.Scraper;

internal class TVMazeScraperService(
    HttpClient httpClient,
    IOptions<TVMazeSettings> settings) : ITVMazeScraperService
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly TVMazeSettings _settings = settings.Value;

    public async Task<TVMazeShowDto?> GetTVShowAsync(int id)
    {
        try
        {
            var requestUri = _settings.TVShowByIdWithEmbedCastApi.Replace("{showId}", id.ToString());

            var response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<TVMazeShowDto>(content, _options);

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Dictionary<int, long>> GetTVShowUpdatesAsync()
    {
        var response = await httpClient.GetAsync(_settings.TVShowsUpdatesApi);
        if (!response.IsSuccessStatusCode)
        {
            return new Dictionary<int, long>();
        }

        var content = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(content))
        {
            return JsonSerializer.Deserialize<Dictionary<int, long>>(content, _options);
        }

        return new Dictionary<int, long>();
    }
}