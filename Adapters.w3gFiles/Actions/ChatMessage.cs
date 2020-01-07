using System;

namespace Adapters.w3gFiles.Actions
{
    public class ChatMessage : GameAction
    {
        public int PlayerId { get; }
        public string Message { get; }

        public ChatMessage(int playerId, string message, TimeSpan occuredOn) : base(occuredOn)
        {
            PlayerId = playerId;
            Message = message;
        }
    }
}