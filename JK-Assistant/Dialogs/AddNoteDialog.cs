using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JK_Assistant
{
    /// <summary>
    /// Dialog class for adding new note
    /// </summary>
    public class AddNoteDialog : ComponentDialog
    {
        private const string _noteTitlePrompt = "Please enter your note title";
        private const string _noteTitleInvalid = "Note title must have between 3 and 20 characters. Please enter correct value";
        private const string _noteBodyPrompt = "Please enter your note";
        private const string _shouldSavePrompt = "Would you like to save this note?";
        private const string _titlePromptName = "TitlePrompt";
        private const string _bodyPromptName = "BodyPrompt";
        private const string _titleFieldName = "TitleValue";
        private const string _bodyFieldName = "BodyValue";

        protected readonly UserState UserState;

        public AddNoteDialog(UserState userState)
        {
            InitialDialogId = nameof(MainDialog);
            UserState = userState;

            AddDialog(new TextPrompt(_titlePromptName, TitlePromptValidatorAsync));
            AddDialog(new TextPrompt(_bodyPromptName));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            //main waterfall dialog
            AddDialog(new WaterfallDialog(nameof(MainDialog), new WaterfallStep[]
            {
                GetNoteTitleStepAsync,
                GetNoteBodyStepAsync,
                ConfirmNoteStepAsync,
                EndDialogStepAsync,
            }));
        }

        /// <summary>
        /// Step with prompt for note title
        /// </summary>
        private async Task<DialogTurnResult> GetNoteTitleStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(_titlePromptName,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(_noteTitlePrompt),
                    RetryPrompt = MessageFactory.Text(_noteTitleInvalid),
                }, cancellationToken);
        }

        /// <summary>
        /// Step to store title and prompt for note body
        /// </summary>
        private async Task<DialogTurnResult> GetNoteBodyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[_titleFieldName] = (string)stepContext.Result;

            return await stepContext.PromptAsync(_bodyPromptName,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(_noteBodyPrompt),
                }, cancellationToken);
        }

        /// <summary>
        /// Step to store body and display confirmation
        /// </summary>
        private async Task<DialogTurnResult> ConfirmNoteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[_bodyFieldName] = (string)stepContext.Result;

            var NoteCardAttachment = MessageFactory.Attachment(Functions.CreateNoteCardAttachment((string)stepContext.Values[_titleFieldName],
                (string)stepContext.Values[_bodyFieldName]));

            await stepContext.Context.SendActivityAsync(NoteCardAttachment);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(_shouldSavePrompt),
                }, cancellationToken);
        }

        /// <summary>
        /// Step to save note and end dialog
        /// </summary>
        private async Task<DialogTurnResult> EndDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        /// <summary>
        /// Validator which is limiting the length of title between 3 and 20 chars
        /// </summary>
        private static Task<bool> TitlePromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(promptContext.Recognized.Value.Length >= 3 &&
                promptContext.Recognized.Value.Length <= 20);
        }
    }
}
