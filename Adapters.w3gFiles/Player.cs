namespace Adapters.w3gFiles
{
    public class Player
    {
        public Player(string name, uint playerId, Race race, GameMode gameType)
        {
            Name = name;
            PlayerId = playerId;
            Race = race;
            GameType = gameType;
        }

        public uint PlayerId { get; }
        public Race Race { get; }
        public GameMode GameType { get; }
        public string Name { get; }
    }

    public class GameOwner : Player
    {
        public GameOwner(string name, uint playerId, Race race, GameMode gameType) : base(name, playerId, race, gameType)
        {
        }
    }
}