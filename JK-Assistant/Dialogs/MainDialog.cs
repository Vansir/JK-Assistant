﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Threading;

namespace JK_Assistant
{
    public class MainDialog : InterruptionDialog
    {

        private const string _choicePromptText = "How can I help you?";
        private const string _choiceInvalidText = "Choice is not valid";

        private readonly Dictionary<string, string> _choices = new()
        {
            {"Add note", nameof(AddNoteDialog) },
            {"Read notes", nameof(ReadNotesDialog) },
            {"Search the web", nameof(SearchWebDialog) },
        };

        protected readonly ConversationState ConversationState;
        protected readonly UserState UserState;

        public MainDialog (ConversationState conversationState, UserState userState)
            : base(nameof(MainDialog))
        {
            InitialDialogId = nameof(MainDialog);
            ConversationState = conversationState;
            UserState = userState;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new AddNoteDialog(UserState));
            AddDialog(new ReadNotesDialog(UserState));
            AddDialog(new SearchWebDialog());

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
                    Prompt = MessageFactory.Text(_choicePromptText),
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
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(_choiceInvalidText), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        /// <summary>
        /// Step where bot starts the dialog from the begginning
        /// </summary>
        private async Task<DialogTurnResult> ReplaceDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }
    }
}
