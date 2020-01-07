using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Adapters.w3gFiles.Actions;
using Adapters.w3gFiles.Actions.Leavings;

namespace Adapters.w3gFiles
{
    public class W3GFileReader
    {
        private readonly string _filePath;
        private readonly W3GFileMapping _mapping;
        private readonly IWinnerDeclarer _winnerDeclarer;

        public W3GFileReader(string filePath)
        {
            _filePath = filePath;
            _mapping = new W3GFileMapping();
            _winnerDeclarer = new WinnerDeclarer();
        }

        public Wc3Game Read()
        {
            var fileBytes = File.ReadAllBytes(_filePath);
            return ReadW33Game(fileBytes);
        }

        private Wc3Game ReadW33Game(byte[] fileBytes)
        {
            _mapping.SetBytes(fileBytes);

            var expansionType = _mapping.GetExpansionType();
            var version = _mapping.GetVersion();
            var isMultiPlayer = _mapping.GetIsMultiPlayer();
            var time = _mapping.GetPlayedTime();

            var gameMetaData = _mapping.GetGameMetaData();
            var actions = _mapping.GetActions();

            var allPlayers = new List<Player> {gameMetaData.GameOwner};
            allPlayers.AddRange(gameMetaData.Players);
            var playersWithTeam = MergeTeams(allPlayers, gameMetaData.GameSlots).ToList();

            var gameActions = actions.ToList();
            var chatMessages = gameActions.Where(action => action.GetType() == typeof(ChatMessage))
                .Select(mes => (ChatMessage) mes);
            var leftMessages = gameActions.Where(action => action.GetType() == typeof(PlayerLeft))
                .Select(mes => (PlayerLeft) mes);
            var winners = _winnerDeclarer.GetWinners(leftMessages, playersWithTeam, gameMetaData.GameOwner.PlayerId);
            return new Wc3Game(gameMetaData.GameOwner, expansionType, version, isMultiPlayer, time,
                gameMetaData.GameType, gameMetaData.Map,
                playersWithTeam, gameMetaData.GameSlots, chatMessages, winners);
        }

        private IEnumerable<Player> MergeTeams(IEnumerable<Player> allPlayers, IEnumerable<GameSlot> gameSlots)
        {
            var slots = gameSlots.ToList();
            var allRealPlayers = allPlayers.Where(p => p.IsAdditionalPlayer || p.GetType() == typeof(GameOwner));
            foreach (var player in allRealPlayers)
            {
                var gameSlot = slots.First(slot => slot.PlayerId == player.PlayerId);
                var updatedPlayer = new Player(player.Name, player.PlayerId, player.Race, player.GameType, player.IsAdditionalPlayer,
                    gameSlot.TeamNumber);
                yield return updatedPlayer;
            }
        }
    }
}