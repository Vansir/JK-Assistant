using System.Text.Json.Serialization;

namespace JK_Assistant
{
    public class SearchResultItem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("link")]
        public string Link { get; set; } = "";

        [JsonPropertyName("displayLink")]
        public string DisplayLink { get; set; } = "";

        [JsonPropertyName("snippet")]
        public string Snippet { get; set; } = "Open link to display more details";

        [JsonPropertyName("cacheId")]
        public string CacheId { get; set; } = "";

        [JsonPropertyName("formattedUrl")]
        public string FormattedUrl { get; set; } = "";
    }
}
