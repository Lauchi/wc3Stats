using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Adapters.w3gFiles
{
    public class W3GFileReader
    {
        private readonly W3GFileMapping _mapping;

        public W3GFileReader(W3GFileMapping mapping)
        {
            _mapping = mapping;
        }

        public async Task<Wc3Game> Read(string filePath)
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            _mapping.SetBytes(fileBytes);

            var expansionType = _mapping.GetExpansionType();
            var version = _mapping.GetVersion();
            var isMultiPlayer = _mapping.GetIsMultiPlayer();
            var time = _mapping.GetPlayedTime();

            var host = _mapping.GetGameMetaData();
            var actions = _mapping.GetActions();

            var allPlayers = new List<Player> {host.GameOwner};
            allPlayers.AddRange(host.Players);

            var gameActions = actions.ToList();
            var chatMessages = gameActions.Where(action => action.GetType() == typeof(ChatMessage)).Select(mes => (ChatMessage) mes);
            var leftMessages = gameActions.Where(action => action.GetType() == typeof(PlayerLeft)).Select(mes => (PlayerLeft) mes);
            var winners = GetWinners(leftMessages, allPlayers);
            return new Wc3Game(host.GameOwner, expansionType, version, isMultiPlayer, time, host.GameType, host.Map,
                allPlayers, host.GameSlots, chatMessages, winners);
        }

        private IEnumerable<Player> GetWinners(IEnumerable<PlayerLeft> leftMessages, List<Player> allPlayers)
        {
            return null;
        }
    }
}