namespace HnStoriesRetriever.Services;

public interface IHnService
{
  IEnumerable<Story> Get(int? count);
  Task<IEnumerable<Story>> RefreshList(CancellationToken cancellationToken = default);
}