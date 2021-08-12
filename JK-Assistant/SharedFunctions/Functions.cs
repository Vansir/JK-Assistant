using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace JK_Assistant
{
    public class Functions
    {
        /// <summary>
        /// This function generates Adaptive card to display the note
        /// </summary>
        public static Attachment CreateNoteCardAttachment(string noteTitle, string noteBody)
        {
            //Combine path for cross platform support
            var paths = new[] { ".", "Resources", "NoteAdaptiveCard.txt" };
            var noteCardJson = File.ReadAllText(Path.Combine(paths));
            dynamic cardJsonObject = JsonConvert.DeserializeObject(noteCardJson);

            //Update the values of title and note inside of adaptive card
            cardJsonObject["body"][0]["columns"][0]["items"][0]["text"] = noteTitle;
            cardJsonObject["body"][1]["text"] = noteBody;

            //Create the attachment
            var noteCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = cardJsonObject,
            };

            return noteCardAttachment;
        }

        public static IMessageActivity GenerateNotesCarousel(List<UserNote> userNotesList)
        {
            IMessageActivity notesCarousel = MessageFactory.Attachment(new List<Attachment>());
            notesCarousel.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            foreach (var note in userNotesList)
            {
                notesCarousel.Attachments.Add(CreateNoteCardAttachment(note.NoteTitle, note.NoteBody));
            }

            return notesCarousel;
        }
    }
}
