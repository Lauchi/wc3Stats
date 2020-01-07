namespace Adapters.w3gFiles.Actions.Leavings
{
    public class PlayerLeft : GameAction
    {
        public int PlayerId { get; }
        public uint UnknownWinFlag { get; }
        public LeftReason Reason { get; }
        public LeftResult Result { get; }

        public PlayerLeft(int playerId, LeftReason reason, LeftResult result, uint unknownWinFlag)
        {
            PlayerId = playerId;
            Reason = reason;
            Result = result;
            UnknownWinFlag = unknownWinFlag;
        }

        public PlayerLeft(int playerId, uint reason, uint result, uint unknownWinFlag)
        {
            PlayerId = playerId;

            switch (reason)
            {
                case 0x01: Reason = LeftReason.ConnectionClosedByServer;
                    break;
                case 0x0C: Reason = LeftReason.ConnectionClosedByGame;
                    break;
                case 0x0E: Reason = LeftReason.Undefined;
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

            UnknownWinFlag = unknownWinFlag;
        }
    }
}