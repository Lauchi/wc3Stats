namespace Adapters.w3gFiles
{
    public class ChatMessage : IGameAction
    {
        public int PlayerId { get; }
        public string Message { get; }

        public ChatMessage(int playerId, string message)
        {
            PlayerId = playerId;
            Message = message;
        }
    }
}