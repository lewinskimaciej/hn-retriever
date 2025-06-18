using HnStoriesRetriever.HackerNews;

namespace HnStoriesRetriever.Queries;

public interface IGetBestStoriesQuery
{
  Task<IEnumerable<long>> Handle(CancellationToken cancellationToken = default);
}

public class GetBestStoriesQuery : IGetBestStoriesQuery
{
  private readonly HnHttpClient _client;

  public GetBestStoriesQuery(HnHttpClient client)
  {
    _client = client;
  }

  public async Task<IEnumerable<long>> Handle(CancellationToken cancellationToken = default)
  {
    var bestStories = await _client.GetBestStoriesAsync(cancellationToken);
    // todo: cache shortly?
    // todo: consider erroring out instead of empty array?
    return bestStories ?? [];
  }
}