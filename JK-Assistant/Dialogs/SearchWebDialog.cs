using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JK_Assistant
{
    public class SearchWebDialog : ComponentDialog
    {
        private const string _webSearchCardPrompt = "WebSearchCard";
        private const string _googleSearchUrlPrefix = "https://www.google.com/search?q=";
        public SearchWebDialog()
        {
            InitialDialogId = nameof(SearchWebDialog);

            AddDialog(new TextPrompt(_webSearchCardPrompt,WebSearchCardValidatorAsync));

            //main waterfall dialog
            AddDialog(new WaterfallDialog(nameof(SearchWebDialog), new WaterfallStep[]
            {
                SearchCardStepAsync,
                DisplayWebpage,
            }));
        }

        private async Task<DialogTurnResult> SearchCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Create Adaptive Card with web search
            Attachment webSearchCardAttachment = Functions.CreateWebSearchCardAttachment();

            //Return prompt with adaptive card
            return await stepContext.PromptAsync(_webSearchCardPrompt,
                new PromptOptions
                {
                    Prompt = (Activity)MessageFactory.Attachment(webSearchCardAttachment),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayWebpage(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Get user choice.
            JObject jObject = stepContext.Context.Activity.Value as JObject ?? null;

            if (!(jObject is null))
            {
                //Store value to search for
                string searchValue = (string)jObject["SearchValue"];

                //Build url to use
                string searchUrl = _googleSearchUrlPrefix + searchValue;

                var searchResultCard = MessageFactory.Attachment(Functions.CreateSearchResultCardAttachment(searchUrl));

                await stepContext.Context.SendActivityAsync(searchResultCard);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Task<bool> WebSearchCardValidatorAsync(PromptValidatorContext<string> promptValidatorContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
