namespace HnStoriesRetriever.HackerNews;

public interface IHnHttpClient
{
  Task<IEnumerable<long>?> GetBestStoriesAsync(CancellationToken cancellationToken = default);
  Task<HnItem?> GetItemAsync(long itemId, CancellationToken cancellationToken = default);
}