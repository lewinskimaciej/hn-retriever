namespace HnStoriesRetriever.Services;

public class HnService(IHnHttpClient client) : IHnService
{
  private readonly IHnHttpClient _client = client ?? throw new ArgumentNullException(nameof(client));
  // we could use MemoryCache or DistributedCache but this will be faster and there's not that much data anyway
  private ICollection<Story> _cachedStories = [];

  public IEnumerable<Story> Get(int? count)
  {
    return  count.HasValue ? _cachedStories.Take(count.Value) : _cachedStories;
  }

  public async Task<IEnumerable<Story>> RefreshList(CancellationToken cancellationToken = default)
  {
    var bestStories = await _client.GetBestStoriesAsync(cancellationToken);
    if (bestStories == null)
    {
      return [];
    }

    var itemTasks = bestStories
      .Select(storyId => _client.GetItemAsync(storyId, cancellationToken))
      .ToList();

    await Task.WhenAll(itemTasks);
    
    var mappedItems = itemTasks
      .Select(task => task.Result)
      .Where(item => item != null)
      .Cast<HnItem>()
      .OrderByDescending(item => item.Score)
      .Select(MapItemToStory);
    
    _cachedStories = mappedItems.ToList();
    
    return _cachedStories;
  }

  private static Story MapItemToStory(HnItem item)
  {
    return new Story
    {
      Title = item.Title,
      Uri = item.Url,
      PostedBy = item.By,
      Time = DateTimeOffset.FromUnixTimeSeconds(item.Time),
      Score = item.Score,
      CommentCount = item.Descendants
    };
  }
}