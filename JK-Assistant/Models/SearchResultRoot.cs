using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JK_Assistant
{
    class SearchResultRoot
    {
        [JsonPropertyName("items")]
        public List<SearchResultItem> Items { get; set; }
    }
}
