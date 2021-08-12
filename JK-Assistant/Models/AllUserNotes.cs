using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JK_Assistant
{
    public class AllUserNotes
    {
        public List<UserNote> UserNotesList { get; set; }

        public AllUserNotes()
        {
            UserNotesList = new List<UserNote>();
        }
    }
}
