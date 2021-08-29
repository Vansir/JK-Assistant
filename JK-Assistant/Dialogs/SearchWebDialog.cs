using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace JK_Assistant
{
    public class SearchWebDialog : ComponentDialog
    {
        private const string _webSearchMessage = "WebSearchCard";
        private readonly IConfiguration _config;
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
            var searchValue = (string)stepContext.Result;
            string googleSearchKey = _config.GetValue<string>("GoogleSearchApiKey");
            string googleSearchEngine = _config.GetValue<string>("GoogleSearchEngineID");
            var resultsObject = await Functions.SearchResultsWithAPI(searchValue, 3, googleSearchKey, googleSearchEngine);

            var searchResultCard = MessageFactory.Attachment(CardsCreationFunctions.CreateSearchResultCardAttachment(searchValue, resultsObject));

            await stepContext.Context.SendActivityAsync(searchResultCard);
        
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
