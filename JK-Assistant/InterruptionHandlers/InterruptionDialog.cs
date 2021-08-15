﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace JK_Assistant
{
    public class InterruptionDialog : ComponentDialog
    {
        public InterruptionDialog (string id)
            : base(id)
        {
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            var result = await InterruptionAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptionAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text?.ToLowerInvariant();

                switch (text)
                {
                    case "help":
                        HeroCard helpHeroCard = Functions.GenerateHelpCard();
                        var helpMessage = MessageFactory.Attachment(helpHeroCard.ToAttachment());
                        await innerDc.Context.SendActivityAsync(helpMessage, cancellationToken);
                        await innerDc.RepromptDialogAsync(cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);

                    case "exit":
                        await innerDc.Context.SendActivityAsync("Goodbye", cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync(cancellationToken);
                }
            }
            return null;
        }
    }
}
