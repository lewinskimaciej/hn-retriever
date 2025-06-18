using HnStoriesRetriever.Models;

namespace HnStoriesRetriever.Services;

public interface IHnService
{
  Task<IEnumerable<Story>> GetBestStoriesAsync(int? topCount = null);
}