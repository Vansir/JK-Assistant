using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Threading;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace JK_Assistant
{
    public class MainDialog : InterruptionDialog
    {
        private Templates _templates;

        private readonly Dictionary<string, string> _choices = new()
        {
            {"Add note", nameof(AddNoteDialog) },
            {"Read notes", nameof(ReadNotesDialog) },
            {"Search the web", nameof(SearchWebDialog) },
        };

        protected readonly ConversationState ConversationState;
        protected readonly UserState UserState;
        private readonly IConfiguration _config;

        public MainDialog (ConversationState conversationState, UserState userState, IConfiguration config)
            : base(nameof(MainDialog))
        {
            _config = config;
            InitialDialogId = nameof(MainDialog);
            ConversationState = conversationState;
            UserState = userState;

            string[] paths = { ".", "Resources", "LanguageGeneration.lg" };
            string fullPath = Path.Combine(paths);
            _templates = Templates.ParseFile(fullPath);

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new AddNoteDialog(UserState));
            AddDialog(new ReadNotesDialog(UserState));
            AddDialog(new SearchWebDialog(_config));

            //main waterfall dialog
            AddDialog(new WaterfallDialog(nameof(MainDialog), new WaterfallStep[]
            {
                ActionSelectStepAsync,
                DialogSelectStepAsync,
                ReplaceDialogStepAsync,
            }));    
        }

        /// <summary>
        /// Step where prompt is displayed with selection of possible actions
        /// </summary>
        private async Task<DialogTurnResult> ActionSelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = ActivityFactory.FromObject(_templates.Evaluate("MenuMessage")),
                    RetryPrompt = ActivityFactory.FromObject(_templates.Evaluate("InvalidChoice")),
                    Choices = ChoiceFactory.ToChoices(_choices.Keys.ToList()),
                }, cancellationToken);
        }

        /// <summary>
        /// Step where bot starts dialog based on user selection
        /// </summary>
        private async Task<DialogTurnResult> DialogSelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userChoice = ((FoundChoice)stepContext.Result).Value;

            if (_choices.ContainsKey(userChoice))
            {
                return await stepContext.BeginDialogAsync(_choices[userChoice], null, cancellationToken);
            }
            await stepContext.Context.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("ChoiceNotImplemented")), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        /// <summary>
        /// Step where bot starts the dialog from the begginning.
        /// </summary>
        private async Task<DialogTurnResult> ReplaceDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }
    }
}
