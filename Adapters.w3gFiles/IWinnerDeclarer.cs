using System.Collections.Generic;
using Adapters.w3gFiles.Actions.Leavings;

namespace Adapters.w3gFiles
{
    public interface IWinnerDeclarer
    {
        IEnumerable<Player> GetWinners(IEnumerable<PlayerLeft> leftMessages, IEnumerable<Player> allPlayers, uint playerSavedId);
    }
}