using HnStoriesRetriever.HackerNews.Models;

namespace HnStoriesRetriever.HackerNews;

// todo: handle rate-limiting, 429s etc
public class HnHttpClient: HttpClient
{
    private readonly HttpClient _httpClient;

    public HnHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<IEnumerable<long>?> GetBestStoriesAsync()
    {
        var response = await _httpClient.GetAsync("topstories.json");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<long>>();
    }

    public async Task<Item?> GetItemAsync(long itemId)
    {
        var response = await _httpClient.GetAsync($"items/{itemId}.json");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Item>();
    }
}