using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JK_Assistant
{
    public class ReadNotesDialog : ComponentDialog
    {
        private const string _noNotesMessage= "No notes were added yet";

        //Accessor for AllUserNotes used to save the data
        private readonly IStatePropertyAccessor<UserNotes> _allUserNotesAccessor;
        public ReadNotesDialog(UserState userState)
        {
            InitialDialogId = nameof(ReadNotesDialog);
            _allUserNotesAccessor = userState.CreateProperty<UserNotes>(nameof(UserNotes));
            
            //main waterfall dialog
            AddDialog(new WaterfallDialog(nameof(ReadNotesDialog), new WaterfallStep[]
            {
                DisplayNotesStepAsync,
            }));
        }

        /// <summary>
        /// This step displays all notes in a carousel or cards
        /// </summary>
        private async Task<DialogTurnResult> DisplayNotesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var AllUserNotes = await _allUserNotesAccessor.GetAsync(stepContext.Context, () => new UserNotes(), cancellationToken);

            //If there are notes, display them, else send message with information.
            if (AllUserNotes.Notes.Any())
            {
                IMessageActivity notesCarousel = Functions.GenerateNotesCarousel(AllUserNotes.Notes);
                await stepContext.Context.SendActivityAsync(notesCarousel, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(_noNotesMessage);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
