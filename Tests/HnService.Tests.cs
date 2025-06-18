using HnStoriesRetriever.HackerNews.Models;
using HnStoriesRetriever.Queries;
using HnStoriesRetriever.Services;
using Moq;

namespace Tests;

public class HnServiceTests
{
    private static List<long> GetStoryIds(int count) => Enumerable.Range(1, count).Select(i => (long)i).ToList();

    private static HnItem CreateHnItem(long id) => new HnItem
    {
        Id = id,
        Title = $"Title {id}",
        Url = $"http://story{id}.com",
        By = $"user{id}",
        Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Score = Random.Shared.Next(0, 10_000),
        Descendants = Random.Shared.Next(0, 1_000),
        Kids = [],
        Type = "story"
    };

    [Fact]
    public async Task GetBestStoriesAsync_NoTopCount_ReturnsAll()
    {
        // Arrange
        var storyIds = GetStoryIds(10);
        var getBestStoriesQuery = new Mock<IGetBestStoriesQuery>();
        getBestStoriesQuery.Setup(q => q.Handle(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyIds);

        var getItemQuery = new Mock<IGetItemQuery>();
        foreach (var id in storyIds)
        {
            getItemQuery.Setup(q => q.Handle(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateHnItem(id));
        }

        var service = new HnService(getBestStoriesQuery.Object, getItemQuery.Object);

        // Act
        var result = (await service.GetBestStoriesAsync()).ToList();

        // Assert
        Assert.Equal(10, result.Count);
        foreach (var story in result)
        {
            Assert.StartsWith("Title", story.Title);
        }
    }

    [Fact]
    public async Task GetBestStoriesAsync_TopCount5_Returns5()
    {
        // Arrange
        var storyIds = GetStoryIds(10);
        var getBestStoriesQuery = new Mock<IGetBestStoriesQuery>();
        getBestStoriesQuery.Setup(q => q.Handle(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storyIds);

        var getItemQuery = new Mock<IGetItemQuery>();
        foreach (var id in storyIds)
        {
            getItemQuery.Setup(q => q.Handle(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateHnItem(id));
        }

        var service = new HnService(getBestStoriesQuery.Object, getItemQuery.Object);

        // Act
        var result = (await service.GetBestStoriesAsync(5)).ToList();

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetBestStoriesAsync_TopCount500_ReturnsAllAvailable()
    {
      // Arrange
      var storyIds = GetStoryIds(10);
      var getBestStoriesQuery = new Mock<IGetBestStoriesQuery>();
      getBestStoriesQuery.Setup(q => q.Handle(It.IsAny<CancellationToken>()))
        .ReturnsAsync(storyIds);

      var getItemQuery = new Mock<IGetItemQuery>();
      foreach (var id in storyIds)
      {
        getItemQuery.Setup(q => q.Handle(id, It.IsAny<CancellationToken>()))
          .ReturnsAsync(CreateHnItem(id));
      }

      var service = new HnService(getBestStoriesQuery.Object, getItemQuery.Object);

      // Act
      var result = (await service.GetBestStoriesAsync(500)).ToList();

      // Assert
      Assert.Equal(10, result.Count);
    }
    
    [Fact]
    public async Task GetBestStoriesAsync_OrderedProperly()
    {
      // Arrange
      var storyIds = GetStoryIds(10);
      var getBestStoriesQuery = new Mock<IGetBestStoriesQuery>();
      getBestStoriesQuery.Setup(q => q.Handle(It.IsAny<CancellationToken>()))
        .ReturnsAsync(storyIds);

      var getItemQuery = new Mock<IGetItemQuery>();
      foreach (var id in storyIds)
      {
        getItemQuery.Setup(q => q.Handle(id, It.IsAny<CancellationToken>()))
          .ReturnsAsync(CreateHnItem(id));
      }

      var service = new HnService(getBestStoriesQuery.Object, getItemQuery.Object);

      // Act
      var result = (await service.GetBestStoriesAsync(500)).ToList();
      var orderedResult = result.OrderByDescending(s => s.Score).ToList();

      // Assert
      Assert.Equal(result, orderedResult);
    }
}