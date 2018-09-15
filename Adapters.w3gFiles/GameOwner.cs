namespace Adapters.w3gFiles
{
    public class GameOwner
    {
        public GameOwner(string name, uint playerId, GameMode gameType, Race race)
        {
            Name = name;
            PlayerId = playerId;
            GameType = gameType;
            Race = race;
        }

        public uint PlayerId { get; }
        public GameMode GameType { get; }
        public Race Race { get; }
        public string Name { get; }
    }
}