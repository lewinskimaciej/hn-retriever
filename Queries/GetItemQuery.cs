using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using HnStoriesRetriever.HackerNews;
using HnStoriesRetriever.HackerNews.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace HnStoriesRetriever.Queries;

public interface IGetItemQuery
{
  Task<HnItem?> Handle(long itemId, CancellationToken cancellationToken = default);
}

public class GetItemQuery : IGetItemQuery
{
  private readonly IDistributedCache _cache;
  private readonly HnHttpClient _client;
  private readonly DistributedCacheEntryOptions _defaultCacheOptions;

  public GetItemQuery(IDistributedCache cache, HnHttpClient client, IConfiguration configuration)
  {
    _cache = cache;
    _client = client;
    _defaultCacheOptions = new DistributedCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromSeconds(configuration.GetValue("SlidingStoryCacheTimeInSeconds", 30)),
      AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(configuration.GetValue("AbsoluteStoryCacheTimeInSeconds", 120))
    };
  }

  public async Task<HnItem?> Handle(long itemId, CancellationToken cancellationToken = default)
  {
    var cachedValue = await _cache.GetAsync(itemId.ToString(), cancellationToken);
    if (cachedValue is not null)
    {
      var serializedPoco = Encoding.UTF8.GetString(cachedValue);

      //change the type according to the poco object you are using 
      return JsonSerializer.Deserialize<HnItem>(serializedPoco);
    }

    var hnItem = await _client.GetItemAsync(itemId, cancellationToken);
    if (hnItem is null)
    {
      // consider erroring out instead of returning null
      return null;
    }

    var serializedHnItem = JsonSerializer.Serialize(hnItem);
    await _cache.SetAsync(
      itemId.ToString(),
      Encoding.UTF8.GetBytes(serializedHnItem),
      _defaultCacheOptions,
      cancellationToken);

    return hnItem;
  }
}