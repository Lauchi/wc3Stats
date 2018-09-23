namespace Adapters.w3gFiles
{
    public class PlayerLeft : IGameAction
    {
        public int PlayerId { get; }
        public uint UnknownWinFlag { get; }
        public LeftReason Reason { get; }
        public LeftResult Result { get; }

        public PlayerLeft(int playerId, uint reason, uint result, uint unknownWinFlag)
        {
            PlayerId = playerId;
            UnknownWinFlag = unknownWinFlag;
            switch (reason)
            {
                case 0x01: Reason = LeftReason.ConnectionClosedByServer;
                    break;
                case 0x0C: Reason = LeftReason.ConnectionClosedByGame;
                    break;
                case 0x0E: Reason = LeftReason.Unknown;
                    break;
            }

            switch (result)
            {
                case 0x01: Result = LeftResult.PlayerDisconnected;
                    break;
                case 0x07: Result = LeftResult.PlayerLeft;
                    break;
                case 0x08: Result = LeftResult.PlayerWasCompletelyErased;
                    break;
                case 0x09: Result = LeftResult.PlayerWon;
                    break;
                case 0x0A: Result = LeftResult.Draw;
                    break;
                case 0x0B: Result = LeftResult.PlayerLeftAsObserver;
                    break;
            }
        }
    }

    public enum LeftReason
    {
        ConnectionClosedByServer, ConnectionClosedByGame, Unknown
    }

    public enum LeftResult
    {
        PlayerDisconnected, PlayerLeft, PlayerWasCompletelyErased, PlayerWon, Draw, PlayerLeftAsObserver
    }
}