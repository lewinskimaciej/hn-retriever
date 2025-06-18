using HnStoriesRetriever.HackerNews;
using HnStoriesRetriever.HackerNews.Models;
using HnStoriesRetriever.Models;
using HnStoriesRetriever.Queries;

namespace HnStoriesRetriever.Services;

public class HnService : IHnService
{
  private readonly IGetBestStoriesQuery _getBestStoriesQuery;
  private readonly IGetItemQuery _getItemQuery;

  public HnService(IGetBestStoriesQuery getBestStoriesQuery, IGetItemQuery getItemQuery)
  {
    _getBestStoriesQuery = getBestStoriesQuery;
    _getItemQuery = getItemQuery;
  }

  public async Task<IEnumerable<Story>> GetBestStoriesAsync(int? topCount = null)
  {
    var bestStories = (await _getBestStoriesQuery.Handle()).ToList();
    var storiesCount = bestStories.Count;

    IEnumerable<long> storiesToTake = bestStories;
    if (topCount.HasValue)
    {
      storiesToTake = storiesToTake.Take(Math.Min(topCount.Value, storiesCount));
    }

    var itemTasks = storiesToTake
      .Select(storyId => _getItemQuery.Handle(storyId))
      .ToList();

    await Task.WhenAll(itemTasks);

    return itemTasks
      .Select(task => task.Result)
      .Where(story => story != null)
      .Cast<HnItem>()
      .Select(MapItemToStory);
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