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
            var winners = GetWinners(leftMessages, allPlayers);
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
                player.SetTeam(gameSlot.TeamNumber);
                yield return player;
            }
        }

        private IEnumerable<Player> GetWinners(IEnumerable<PlayerLeft> leftMessages, List<Player> allPlayers)
        {
            var playerLefts = leftMessages.ToList();
            var playerLeftLast = playerLefts.Last();
            switch (playerLeftLast.Reason)
            {
                case LeftReason.ConnectionClosedByGame:
                {
                    switch (playerLeftLast.Result)
                    {
                        case LeftResult.PlayerWon:
                            yield return allPlayers.First(player => player.PlayerId == playerLeftLast.PlayerId);
                            break;
                        case LeftResult.PlayerWasCompletelyErased:
                            yield return allPlayers.First(player => player.PlayerId != playerLeftLast.PlayerId);
                            break;
                        case LeftResult.PlayerLeft:
                        {
                            if (IsWinner(playerLefts))
                            {
                                yield return allPlayers.First(player => player.PlayerId == playerLeftLast.PlayerId);
                            }
                            yield return allPlayers.First(player => player.PlayerId != playerLeftLast.PlayerId);
                            break;
                        }
                    }
                }
                    break;
            }
        }

        private bool IsWinner(List<PlayerLeft> playerLefts)
        {
            var playerLeftLast = playerLefts.Last();
            var playerLeftLastPrevious = playerLefts.AsEnumerable().Reverse().Skip(1).First();
            if (playerLeftLast.UnknownWinFlag == playerLeftLastPrevious.UnknownWinFlag) return false;
            return false;
        }
    }
}