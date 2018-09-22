using System.Collections.Generic;

namespace Adapters.w3gFiles
{
    internal class MapAndPlayers
    {
        public MapAndPlayers(Map map, IEnumerable<Player> players, IEnumerable<GameSlot> gameSlots)
        {
            Map = map;
            Players = players;
            GameSlots = gameSlots;
        }

        public Map Map { get; }
        public IEnumerable<Player> Players { get; }
        public IEnumerable<GameSlot> GameSlots { get; }
    }
}