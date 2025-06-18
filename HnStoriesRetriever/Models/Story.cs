namespace HnStoriesRetriever.Models;

public record Story
{
  [JsonPropertyName("title")] public required string Title { get; set; }

  [JsonPropertyName("uri")] public required string? Uri { get; set; }

  [JsonPropertyName("postedBy")] public required string PostedBy { get; set; }

  [JsonPropertyName("time")] public DateTimeOffset Time { get; set; }

  [JsonPropertyName("score")] public int Score { get; set; }

  [JsonPropertyName("commentCount")] public int CommentCount { get; set; }
}