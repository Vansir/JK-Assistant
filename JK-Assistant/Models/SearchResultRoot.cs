using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JK_Assistant
{
    public class SearchResultRoot
    {
        [JsonPropertyName("items")]
        public List<SearchResultItem> Items { get; set; }

        public SearchResultRoot()
        {
            Items = new List<SearchResultItem>();
        }
    }
}
