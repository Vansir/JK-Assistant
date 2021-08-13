using System.Collections.Generic;

namespace JK_Assistant
{
    public class UserNotes
    {
        public List<UserNote> Notes { get; }

        public UserNotes()
        {
            Notes = new List<UserNote>();
        }
    }
}
