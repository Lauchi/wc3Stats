using System.Collections.Generic;

namespace Adapters.w3gFiles
{
    public interface IWinnerDeclarer
    {
        IEnumerable<Player> GetWinners(IEnumerable<PlayerLeft> leftMessages, IEnumerable<Player> allPlayers, uint playerSavedId);
    }
}