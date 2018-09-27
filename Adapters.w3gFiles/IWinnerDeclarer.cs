using System.Collections.Generic;

namespace Adapters.w3gFiles
{
    public interface IWinnerDeclarer
    {
        IEnumerable<Player> GetWinners(IEnumerable<PlayerLeft> leftMessages, List<Player> allPlayers, uint playerSavedId);
    }
}