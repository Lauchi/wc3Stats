namespace Adapters.w3gFiles
{
    public class GameOwner
    {
        public GameOwner(string name, uint playerId, Race race)
        {
            Name = name;
            PlayerId = playerId;
            Race = race;
        }

        public uint PlayerId { get; }
        public Race Race { get; }
        public string Name { get; }
    }
}