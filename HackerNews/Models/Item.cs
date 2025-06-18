using System.Text.Json.Serialization;

namespace HnStoriesRetriever.HackerNews.Models;

public record Item
{
    [JsonPropertyName("by")] public string By { get; set; }

    [JsonPropertyName("descendants")] public int Descendants { get; set; }

    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("kids")] public long[] Kids { get; set; }

    [JsonPropertyName("score")] public int Score { get; set; }

    [JsonPropertyName("time")] public DateTimeOffset Time { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; }
}