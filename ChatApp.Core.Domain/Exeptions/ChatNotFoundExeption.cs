namespace ChatApp.Core.Domain.Exeptions
{
    public class ChatNotFoundExeption : Exception
    {
        public ChatNotFoundExeption(string chatName)
            : base($"Chat with name '{chatName}' not found")
        {
        }
    }
}
