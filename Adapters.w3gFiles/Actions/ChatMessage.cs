using System;

namespace Adapters.w3gFiles.Actions
{
    public class ChatMessage : GameAction
    {
        public int PlayerId { get; }
        public string Message { get; }
        public ChatChannel Channel { get; }

        public ChatMessage(int playerId, string message, ChatChannel channel, TimeSpan occuredOn) : base(occuredOn)
        {
            PlayerId = playerId;
            Message = message;
            Channel = channel;
        }
    }

    public enum ChatChannel
    {
        Undefined, All, Allies, Observer, Whisper
    }
}