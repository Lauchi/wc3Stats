using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Adapters.w3gFiles
{
    public class W3GFileReader
    {
        private readonly W3GFileMapping _mapping;
        private IWinnerDeclarer _winnerDeclarer;

        public W3GFileReader()
        {
            _mapping = new W3GFileMapping();
            _winnerDeclarer = new WinnerDeclarer();
        }

        public async Task<Wc3Game> Read(string filePath)
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            _mapping.SetBytes(fileBytes);

            var expansionType = _mapping.GetExpansionType();
            var version = _mapping.GetVersion();
            var isMultiPlayer = _mapping.GetIsMultiPlayer();
            var time = _mapping.GetPlayedTime();

            var gameMetaData = _mapping.GetGameMetaData();
            var actions = _mapping.GetActions();

            var allPlayers = new List<Player> {gameMetaData.GameOwner};
            allPlayers.AddRange(gameMetaData.Players);
            var playersWithTeam = MergeTeams(allPlayers, gameMetaData.GameSlots);

            var gameActions = actions.ToList();
            var chatMessages = gameActions.Where(action => action.GetType() == typeof(ChatMessage))
                .Select(mes => (ChatMessage) mes);
            var leftMessages = gameActions.Where(action => action.GetType() == typeof(PlayerLeft))
                .Select(mes => (PlayerLeft) mes);
            var winners = _winnerDeclarer.GetWinners(leftMessages, allPlayers, gameMetaData.GameOwner.PlayerId);
            return new Wc3Game(gameMetaData.GameOwner, expansionType, version, isMultiPlayer, time,
                gameMetaData.GameType, gameMetaData.Map,
                playersWithTeam, gameMetaData.GameSlots, chatMessages, winners);
        }

        private IEnumerable<Player> MergeTeams(List<Player> allPlayers, IEnumerable<GameSlot> gameSlots)
        {
            var players = gameSlots.ToList();
            foreach (var player in allPlayers)
            {
                var gameSlot = players.First(slot => slot.PlayerId == player.PlayerId);
                var updatedPlayer = new Player(player.Name, player.PlayerId, player.Race, player.GameType, player.IsAdditionalPlayer,
                    gameSlot.TeamNumber);
                yield return updatedPlayer;
            }
        }
    }
}