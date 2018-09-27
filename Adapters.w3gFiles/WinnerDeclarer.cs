using System.Collections.Generic;
using System.Linq;

namespace Adapters.w3gFiles
{
    public class WinnerDeclarer : IWinnerDeclarer
    {
        public IEnumerable<Player> GetWinners(IEnumerable<PlayerLeft> leftMessages, List<Player> allPlayers, uint playerSavedID)
        {
            var playerLefts = leftMessages.ToList();
            var gameOwnerLeft = playerLefts.Last();
            var gameOwner = allPlayers.First(player => player.PlayerId == gameOwnerLeft.PlayerId);
            switch (gameOwnerLeft.Reason)
            {
                case LeftReason.ConnectionClosedByGame:
                {
                    switch (gameOwnerLeft.Result)
                    {
                        case LeftResult.PlayerWon:
                            yield return allPlayers.First(player => player.PlayerId == gameOwnerLeft.PlayerId);
                            break;
                        case LeftResult.PlayerWasCompletelyErased:
                            yield return allPlayers.First(player => player.PlayerId != gameOwnerLeft.PlayerId);
                            break;
                        case LeftResult.PlayerLeft:
                        {
                            foreach (var player in allPlayers)
                            {
                                var leftEvent = GetLeftEvent(player, playerLefts);
                                if (player.IsAdditionalPlayer && player.Team != gameOwner.Team)
                                {
                                    if (gameOwnerLeft.UnknownWinFlag > leftEvent.UnknownWinFlag)
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