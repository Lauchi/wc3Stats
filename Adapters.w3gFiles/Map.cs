using System.Linq;

namespace Adapters.w3gFiles
{
    public class Map
    {
        public string GameName { get; }
        public string MapName => MapPath.Split("/").Last().Split(".").First();
        public string MapPath { get; }

        public Map(string gameName, string mapName)
        {
            GameName = gameName;
            MapPath = mapName;
        }
    }
}