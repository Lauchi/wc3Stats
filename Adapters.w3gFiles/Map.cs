namespace Adapters.w3gFiles
{
    public class Map
    {
        public string GameName { get; }
        public string MapName { get; }

        public Map(string gameName, string mapName)
        {
            GameName = gameName;
            MapName = mapName;
        }
    }
}