using System;

namespace Adapters.w3gFiles
{
    public class Wc3Game
    {
        public Wc3Game(GameOwner host, ExpansionType expansionType, GameVersion version,
            PlayerMode playerMode,
            TimeSpan gameTime)
        {
            Host = host;
            ExpansionType = expansionType;
            Version = version;
            PlayerMode = playerMode;
            GameTime = gameTime;
        }

        public ExpansionType ExpansionType { get; }
        public GameVersion Version { get; }
        public PlayerMode PlayerMode { get; }
        public TimeSpan GameTime { get; }
        public GameOwner Host { get; }
    }
}