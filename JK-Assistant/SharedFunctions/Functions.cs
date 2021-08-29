using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace JK_Assistant
{
    public class Functions
    {
        private const string _googleCustomSearchUri = "https://www.googleapis.com/customsearch/v1";
        private readonly IConfiguration _config;

        public Functions(IConfiguration config)
        {
            _config = config;
        }

        public static async Task<SearchResultRoot> GetSearchResults(string searchValue, int numberOfResults, string googleSearchKey, string googleSearchEngine)
        {
            var uriBuilder = new UriBuilder(_googleCustomSearchUri);
            //Use default port
            uriBuilder.Port = -1;
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["key"] = googleSearchKey;
            query["cx"] = googleSearchEngine;
            query["num"] = numberOfResults.ToString();
            query["q"] = searchValue;

            uriBuilder.Query = query.ToString();

            var client = new HttpClient();
            var streamTask = client.GetStreamAsync(uriBuilder.ToString());
            return await JsonSerializer.DeserializeAsync<SearchResultRoot>(await streamTask);
        }
    }
}
