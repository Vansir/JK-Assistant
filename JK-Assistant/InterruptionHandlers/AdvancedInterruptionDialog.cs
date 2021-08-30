using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace JK_Assistant
{
    public class AdvancedInterruptionDialog : BaseInterruptionDialog
    {
        public AdvancedInterruptionDialog(string id)
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
                    case "menu":
                    case "cancel":
                        return await innerDc.CancelAllDialogsAsync(cancellationToken);
                }
            }
            return null;
        }
    }
}
