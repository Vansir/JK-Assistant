using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
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
        private const string _googleSearchUrl = "https://www.google.com/search?q=";
        private readonly IConfiguration _config;

        private static readonly HttpClient client = new HttpClient();
        private Templates _templates;
        public SearchWebDialog()
        {
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
            //Create Adaptive Card with web search
            Attachment webSearchCardAttachment = Functions.CreateWebSearchCardAttachment();

            //Return prompt with adaptive card
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

            //Build url to use
            string searchUrl = _googleSearchUrl + searchValue;

            var searchResultCard = MessageFactory.Attachment(Functions.CreateSearchResultCardAttachment(searchUrl));

            await stepContext.Context.SendActivityAsync(searchResultCard);
        
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Task<bool> WebSearchCardValidatorAsync(PromptValidatorContext<string> promptValidatorContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        private static async Task<SearchResultRoot> GetSearchResults(string searchValue)
        {
            var builder = new UriBuilder(_googleCustomSearchUri);
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["key"] = apiKey;
            query["cx"] = searchEngineID;
            query["num"] = searchResultsNumber;
            query["q"] = searchValue;

            builder.Query = query.ToString();

            var check = builder.ToString();

            var streamTask = client.GetStreamAsync(builder.ToString());
            var searchResults = await JsonSerializer.DeserializeAsync<SearchResultRoot>(await streamTask);

            foreach (var item in searchResults.Items)
            {
                Console.WriteLine(item.title);
            }
        }
    }
}
