namespace HnStoriesRetriever.HackerNews;

// todo: possibly add Polly for retries
public class HnHttpClient : HttpClient, IHnHttpClient
{
  private readonly HttpClient _httpClient;
  private readonly ILogger<HnHttpClient> _logger;

  // lets assume HN Firebase API can handle 100 concurrent requests, I don't know real value
  private readonly SemaphoreSlim _semaphore;

  public HnHttpClient(HttpClient httpClient, ILogger<HnHttpClient> logger, IConfiguration configuration)
  {
    _httpClient = httpClient;
    _logger = logger;

    var hnApiMaxConcurrentRequests = configuration.GetValue("HnApiMaxConcurrentRequests", 100);
    _semaphore = new SemaphoreSlim(hnApiMaxConcurrentRequests);
  }

  public async Task<IEnumerable<long>?> GetBestStoriesAsync(CancellationToken cancellationToken = default)
  {
    await _semaphore.WaitAsync(cancellationToken);
    try
    {
      var response = await _httpClient.GetAsync("v0/topstories.json", cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        // consider different error handling
        _logger.LogError("GetBestStoriesAsync failed with status code {StatusCode}", response.StatusCode);
        return null;
      }
      return await response.Content.ReadFromJsonAsync<IEnumerable<long>>(cancellationToken);
    }
    finally
    {
      _semaphore.Release();
    }
  }

  public async Task<HnItem?> GetItemAsync(long itemId, CancellationToken cancellationToken = default)
  {
    await _semaphore.WaitAsync(cancellationToken);
    try
    {
      var response = await _httpClient.GetAsync($"v0/item/{itemId}.json", cancellationToken);
      
      if (!response.IsSuccessStatusCode)
      {
        // consider different error handling
        _logger.LogError("GetItemAsync failed with status code {StatusCode}", response.StatusCode);
        return null;
      }
      
      return await response.Content.ReadFromJsonAsync<HnItem>(cancellationToken);
    }
    finally
    {
      _semaphore.Release();
    }
  }
}