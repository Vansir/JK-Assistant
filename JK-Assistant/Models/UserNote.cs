namespace JK_Assistant
{
    public class UserNote
    {
        public string Title { get; }
        public string Body { get; }

        public UserNote(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }
}
