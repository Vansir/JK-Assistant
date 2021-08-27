using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using AdaptiveCards.Templating;

namespace JK_Assistant
{
    public class Functions
    {
        private const string _googleSearchUrl = "https://www.google.com/search?q=";
        private const string _searchGoogleMsg = "Display more results";

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
                notesCarousel.Attachments.Add(CreateNoteCardAttachment(note.Title, note.Body));
            }

            return notesCarousel;
        }

        public static Attachment CreateSearchResultCardAttachment(string searchValue, SearchResultRoot results)
        {
            string searchUrl = _googleSearchUrl + searchValue;

            //Combine path for cross platform support
            var paths = new[] { ".", "Resources", "SearchResultCard.txt" };
            var template = new AdaptiveCardTemplate(File.ReadAllText(Path.Combine(paths)));
            var data = results;
            if (data.Items == null) data.Items = new List<SearchResultItem>();
            var googleSearch = new SearchResultItem();

            //Adding option to open google search webpage
            googleSearch.title = _searchGoogleMsg;
            googleSearch.link = searchUrl;
            googleSearch.displayLink = "";
            googleSearch.snippet = "";

            data.Items.Add(googleSearch);

            string cardJsonObject = template.Expand(data);

            //Create the attachment
            var searchResultCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonObject),
            };

            return searchResultCardAttachment;
        }
        public static HeroCard GenerateHelpCard()
        {
            var newLine = Environment.NewLine;

            return new HeroCard
            {
                Title = $"JK Assistant HELP",

                Text = $"Bot supports following commands:{newLine}" +
                $"- Type 'help' to display help card{newLine}" +
                $"- Type 'exit' to end the conversation",
            };
        }
    }
}
