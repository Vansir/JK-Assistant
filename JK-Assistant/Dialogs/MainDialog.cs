using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System.Threading;

namespace JK_Assistant
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ConversationState ConversationState;
        protected readonly UserState UserState;

        public MainDialog (ConversationState conversationState, UserState userState)
        {
            InitialDialogId = nameof(MainDialog);
            ConversationState = conversationState;
            UserState = userState;

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
            return await stepContext.NextAsync(null, cancellationToken);
        }

        /// <summary>
        /// Step where bot starts dialog based on user selection
        /// </summary>
        private async Task<DialogTurnResult> DialogSelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {stepContext.Context.Activity.Text}";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        /// <summary>
        /// Step where bot starts the dialog from the begginning
        /// </summary>
        private async Task<DialogTurnResult> ReplaceDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, cancellationToken);
        }
    }
}
