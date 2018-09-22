using System.Collections.Generic;

namespace Adapters.w3gFiles
{
    public class GameOwnerHeader
    {
        public GameOwner GameOwner { get; }
        public GameMode GameType { get; }
        public Map Map { get; }
        public IEnumerable<Player> Players { get; }
        public IEnumerable<GameSlot> GameSlots { get; }

        public GameOwnerHeader(GameOwner gameOwner, GameMode gameType, Map map, IEnumerable<Player> players, IEnumerable<GameSlot> gameSlots)
        {
            GameOwner = gameOwner;
            GameType = gameType;
            Map = map;
            Players = players;
            GameSlots = gameSlots;
        }
    }
}