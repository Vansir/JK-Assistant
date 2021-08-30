using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace JK_Assistant
{
    /// <summary>
    /// Dialog class for adding new note
    /// </summary>
    public class AddNoteDialog : AdvancedInterruptionDialog
    {
        private const string _noteTitlePrompt = "Please enter your note title";
        private const string _noteTitleInvalid = "Note title must have between 3 and 20 characters. Please enter correct value";
        private const string _noteBodyPrompt = "Please enter your note";
        private const string _shouldSavePrompt = "Would you like to save this note?";
        private const string _noteNotSavedMessage = "Your note was discarded";
        private const string _noteSavedMessage = "Note successfully saved";
        private const string _titlePromptName = "TitlePrompt";
        private const string _bodyPromptName = "BodyPrompt";
        private const string _titleFieldName = "TitleValue";
        private const string _bodyFieldName = "BodyValue";

        private readonly IStatePropertyAccessor<UserNotes> _userNotesAccessor;
        public AddNoteDialog(UserState userState) : base(nameof(AddNoteDialog))
        {
            InitialDialogId = nameof(AddNoteDialog);
            _userNotesAccessor = userState.CreateProperty<UserNotes>(nameof(UserNotes));

            AddDialog(new TextPrompt(_titlePromptName, TitlePromptValidatorAsync));
            AddDialog(new TextPrompt(_bodyPromptName));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            //main waterfall dialog
            AddDialog(new WaterfallDialog(nameof(AddNoteDialog), new WaterfallStep[]
            {
                GetNoteTitleStepAsync,
                GetNoteBodyStepAsync,
                ConfirmNoteStepAsync,
                StoreNoteStepAsync,
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

            //Display Adaptive Card with note content
            var noteCardAttachment = MessageFactory.Attachment(CardsCreationFunctions.CreateNoteCardAttachment((string)stepContext.Values[_titleFieldName],
                (string)stepContext.Values[_bodyFieldName]));

            await stepContext.Context.SendActivityAsync(noteCardAttachment);

            //Prompt if displayed adaptive card containing the note should be saved
            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(_shouldSavePrompt),
                }, cancellationToken);
        }

        /// <summary>
        /// Step to save note and end dialog
        /// </summary>
        private async Task<DialogTurnResult> StoreNoteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userWantsToStoreNote = (bool)stepContext.Result;
            if (userWantsToStoreNote)
            {
                //Create new UserNote object and get All notes object from User State
                var currentNote = new UserNote((string)stepContext.Values[_titleFieldName], (string)stepContext.Values[_bodyFieldName]);
                var allUserNotes = await _userNotesAccessor.GetAsync(stepContext.Context, () => new UserNotes(), cancellationToken);

                //Add note to all notes and save it in User State
                allUserNotes.Notes.Add(currentNote);
                await _userNotesAccessor.SetAsync(stepContext.Context, allUserNotes, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(_noteNotSavedMessage);
            }
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
