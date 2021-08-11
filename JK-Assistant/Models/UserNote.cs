using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JK_Assistant
{

    /// <summary>
    /// Class to store single user note
    /// </summary>
    public class UserNote
    {
        public string NoteTitle { get; }
        public string NoteBody { get; }

        public UserNote(string noteTitle, string noteBody)
        {
            NoteTitle = noteTitle;
            NoteBody = noteBody;
        }
    }
}
