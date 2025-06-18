using HnStoriesRetriever.HackerNews;
using HnStoriesRetriever.HackerNews.Models;
using HnStoriesRetriever.Services;
using Moq;

namespace Tests;

public class HnService_Tests
{
  private static List<long> GetStoryIds(int count) => Enumerable.Range(1, count).Select(i => (long)i).ToList();

  private static HnItem CreateHnItem(long id) => new HnItem
  {
    Id = id,
    Title = $"Title {id}",
    Url = $"http://story{id}.com",
    By = $"user{id}",
    Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    Score = 1000 + (int)id,
    Descendants = 10 + (int)id,
    Kids = [],
    Type = "story"
  };

  [Theory]
  [InlineData(null, 10)]
  [InlineData(1, 1)]
  [InlineData(500, 10)]
  public async Task RefreshList_And_Get_ReturnsExpectedCount(int? count, int expected)
  {
    // Arrange
    var storyIds = GetStoryIds(10);
    var mockClient = new Mock<IHnHttpClient>();
    mockClient.Setup(c => c.GetBestStoriesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(storyIds);

    foreach (var id in storyIds)
    {
      mockClient.Setup(c => c.GetItemAsync(id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(CreateHnItem(id));
    }

    var service = new HnService(mockClient.Object);

    // Act
    await service.RefreshList();
    var result = service.Get(count).ToList();

    // Assert
    Assert.Equal(expected, result.Count);
    foreach (var story in result)
    {
      Assert.StartsWith("Title", story.Title);
    }
  }
}