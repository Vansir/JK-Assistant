using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Text.Json;
using System;
using Microsoft.Extensions.Configuration;

namespace JK_Assistant
{
    public class SearchWebDialog : ComponentDialog
    {
        private const string _webSearchMessage = "WebSearchCard";
        private const string _googleCustomSearchUri = "https://www.googleapis.com/customsearch/v1";
        private readonly IConfiguration _config;

        private static readonly HttpClient client = new HttpClient();
        private Templates _templates;
        public SearchWebDialog(IConfiguration config)
        {
            _config = config;
            InitialDialogId = nameof(SearchWebDialog);

            string[] paths = { ".", "Resources", "LanguageGeneration.lg" };
            string fullPath = Path.Combine(paths);
            _templates = Templates.ParseFile(fullPath);

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            //main waterfall dialog
            AddDialog(new WaterfallDialog(nameof(SearchWebDialog), new WaterfallStep[]
            {
                SearchInputStepAsync,
                DisplayWebpage,
            }));
        }

        private async Task<DialogTurnResult> SearchInputStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = ActivityFactory.FromObject(_templates.Evaluate("InputSearch")),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayWebpage(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Store value to search for
            string searchValue = (string)stepContext.Result;
            var resultsObject = await GetSearchResults(searchValue, 3);

            var searchResultCard = MessageFactory.Attachment(Functions.CreateSearchResultCardAttachment(searchValue, resultsObject));

            await stepContext.Context.SendActivityAsync(searchResultCard);
        
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<SearchResultRoot> GetSearchResults(string searchValue, int numberOfResults)
        {
            var uriBuilder = new UriBuilder(_googleCustomSearchUri);
            //Use default port
            uriBuilder.Port = -1;
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["key"] = _config.GetValue<string>("GoogleSearchApiKey");
            query["cx"] = _config.GetValue<string>("GoogleSearchEngineID");
            query["num"] = numberOfResults.ToString();
            query["q"] = searchValue;

            uriBuilder.Query = query.ToString();

            var streamTask = client.GetStreamAsync(uriBuilder.ToString());
            return await JsonSerializer.DeserializeAsync<SearchResultRoot>(await streamTask);
        }
    }
}
