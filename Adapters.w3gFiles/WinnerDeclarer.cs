using System.Collections.Generic;
using System.Linq;
using Adapters.w3gFiles.Actions.Leavings;

namespace Adapters.w3gFiles
{
    public class WinnerDeclarer : IWinnerDeclarer
    {
        public IEnumerable<Player> GetWinners(IEnumerable<PlayerLeft> leftMessages, IEnumerable<Player> allPlayers, uint playerSavedID)
        {
            var playerLefts = leftMessages.ToList();
            var gameOwnerLeft = playerLefts.First(player => player.PlayerId == playerSavedID);
            var players = allPlayers.ToList();
            var gameOwner = players.First(player => player.PlayerId == gameOwnerLeft.PlayerId);
            switch (gameOwnerLeft.Reason)
            {
                case LeftReason.ConnectionClosedByGame:
                {
                    switch (gameOwnerLeft.Result)
                    {
                        case LeftResult.PlayerWon:
                            yield return players.First(player => player.PlayerId == gameOwnerLeft.PlayerId);
                            break;
                        case LeftResult.PlayerWasCompletelyErased:
                            yield return players.First(player => player.PlayerId != gameOwnerLeft.PlayerId);
                            break;
                        case LeftResult.PlayerLeft:
                        {
                            foreach (var player in players)
                            {
                                var leftEvent = GetLeftEvent(player, playerLefts);
                                if (player.IsAdditionalPlayer && player.Team != gameOwner.Team)
                                {
                                    if (gameOwnerLeft.UnknownWinFlag == leftEvent.UnknownWinFlag)
                                        yield return player;
                                }
                            }
                            break;
                        }
                    }
                }
                    break;
            }
        }

        private PlayerLeft GetLeftEvent(Player playerIt, List<PlayerLeft> playerLefts)
        {
            var playerLeft = playerLefts.First(p => p.PlayerId == playerIt.PlayerId);
            return playerLeft;
        }
    }
}