namespace Adapters.w3gFiles
{
    public class Player
    {
        public Player(string name, uint playerId, Race race, GameMode gameType, bool isAdditionalPlayer)
        {
            Name = name;
            PlayerId = playerId;
            Race = race;
            GameType = gameType;
            IsAdditionalPlayer = isAdditionalPlayer;
        }

        public Player(string name, uint playerId, Race race, GameMode gameType, bool isAdditionalPlayer, int team)
        {
            Name = name;
            PlayerId = playerId;
            Race = race;
            GameType = gameType;
            IsAdditionalPlayer = isAdditionalPlayer;
            Team = team;
        }

        public uint PlayerId { get; }
        public Race Race { get; }
        public GameMode GameType { get; }
        public bool IsAdditionalPlayer { get; }
        public bool IsReplayOwner => !IsAdditionalPlayer;
        public string Name { get; }
        public int Team { get; }
    }

    public class GameOwner : Player
    {
        public GameOwner(string name, uint playerId, Race race, GameMode gameType, bool isAdditionalPlayer) : base(name, playerId, race, gameType, isAdditionalPlayer)
        {
        }
    }
}