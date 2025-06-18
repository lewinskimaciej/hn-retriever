namespace HnStoriesRetriever.HackerNews.Models;

public record HnItem
{
  [JsonPropertyName("by")] public required string By { get; set; }

  [JsonPropertyName("descendants")] public int Descendants { get; set; }

  [JsonPropertyName("id")] public long Id { get; set; }

  [JsonPropertyName("kids")] public long[] Kids { get; set; } = [];

  [JsonPropertyName("score")] public int Score { get; set; }

  [JsonPropertyName("time")] public long Time { get; set; }

  [JsonPropertyName("title")] public required string Title { get; set; }

  [JsonPropertyName("type")] public required string Type { get; set; }

  [JsonPropertyName("url")] public string? Url { get; set; }
}