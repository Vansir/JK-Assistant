using Microsoft.Bot.Schema;
using Newtonsoft.Json;
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
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "NoteAdaptiveCard.txt" };
            var noteCardJson = File.ReadAllText(Path.Combine(paths));

            noteCardJson = noteCardJson.Replace("<TitleValue>", noteTitle);
            noteCardJson = noteCardJson.Replace("<BodyValue>", noteBody);

            var noteCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(noteCardJson),
            };

            return noteCardAttachment;
        }
    }
}
