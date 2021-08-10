using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Threading;

namespace JK_Assistant
{
    public class MainDialog : ComponentDialog
    {

        private const string _choicePromptText = "How can I help you?";
        private const string _choiceAddNoteText = "Add note";
        private const string _choiceReadNotesText = "Read notes";
        private const string _choiceSearchWebText = "Search the web";
        private const string _choiceInvalidText = "Choice is not valid";

        protected readonly ConversationState ConversationState;
        protected readonly UserState UserState;

        public MainDialog (ConversationState conversationState, UserState userState)
        {
            InitialDialogId = nameof(MainDialog);
            ConversationState = conversationState;
            UserState = userState;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

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
                    Choices = ChoiceFactory.ToChoices(new List<string> { _choiceAddNoteText, _choiceReadNotesText, _choiceSearchWebText }),
                }, cancellationToken);
        }

        /// <summary>
        /// Step where bot starts dialog based on user selection
        /// </summary>
        private async Task<DialogTurnResult> DialogSelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case _choiceAddNoteText:
                    return await stepContext.BeginDialogAsync(nameof(AddNoteDialog), null, cancellationToken);

                //case _choiceReadNotesText:
                //    break;

                //case _choiceSearchWebText:
                //    break;

                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(_choiceInvalidText), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
            }
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
