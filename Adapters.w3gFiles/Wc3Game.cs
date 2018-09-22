using System;
using System.Collections.Generic;

namespace Adapters.w3gFiles
{
    public class Wc3Game
    {
        public Wc3Game(GameOwner host, ExpansionType expansionType, GameVersion version,
            PlayerMode playerMode,
            TimeSpan gameTime, GameMode gameType, Map map, IEnumerable<Player> players, IEnumerable<GameSlot> gameSlots)
        {
            Host = host;
            ExpansionType = expansionType;
            Version = version;
            PlayerMode = playerMode;
            GameTime = gameTime;
            GameType = gameType;
            Map = map;
            Players = players;
            GameSlots = gameSlots;
        }

        public ExpansionType ExpansionType { get; }
        public GameVersion Version { get; }
        public GameMode GameType { get; }
        public PlayerMode PlayerMode { get; }
        public TimeSpan GameTime { get; }
        public GameOwner Host { get; }
        public Map Map { get; }
        public IEnumerable<Player> Players { get; }
        public IEnumerable<GameSlot> GameSlots { get; }
    }
}