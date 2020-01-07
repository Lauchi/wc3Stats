using System.Collections.Generic;
using System.Linq;
using Adapters.w3gFiles.Actions.Leavings;
using NUnit.Framework;

namespace Adapters.w3gFiles.Tests
{
    public class WinnerDeclarerTests
    {
        [Test]
        public void TestMultiplayers()
        {
            var w3GFileReader = new WinnerDeclarer();
            var players = new List<Player>
            {
                new Player("TEst1", 1, Race.Human, GameMode.Ladder, false, 1),
                new Player("TEst2", 2, Race.Human, GameMode.Ladder, true, 2),
                new Player("TEst3", 3, Race.Human, GameMode.Ladder, true, 1),
                new Player("TEst4", 4, Race.Human, GameMode.Ladder, true, 2)
            };

            var winnerEvents = new List<PlayerLeft>
            {
                new PlayerLeft(4, LeftReason.ConnectionClosedByGame, LeftResult.PlayerLeft, 10, default),
                new PlayerLeft(2, LeftReason.ConnectionClosedByGame, LeftResult.PlayerLeft, 10, default),
                new PlayerLeft(3, LeftReason.ConnectionClosedByGame, LeftResult.PlayerLeft, 10, default),
                new PlayerLeft(1, LeftReason.ConnectionClosedByGame, LeftResult.PlayerLeft, 10, default)
            };

            var winners = w3GFileReader.GetWinners(winnerEvents, players, 1).ToList();
            Assert.AreEqual(2, winners[0].PlayerId);
            Assert.AreEqual(4, winners[1].PlayerId);
        }

        [Test]
        public void Test1v1_SaverLeftFirst()
        {
            var w3GFileReader = new WinnerDeclarer();
            var players = new List<Player>
            {
                new Player("TEst1", 1, Race.Human, GameMode.Ladder, false, 1),
                new Player("TEst2", 2, Race.Human, GameMode.Ladder, true, 2),
            };

            var winnerEvents = new List<PlayerLeft>
            {
                new PlayerLeft(2, LeftReason.ConnectionClosedByGame, LeftResult.PlayerLeft, 10, default),
                new PlayerLeft(1, LeftReason.ConnectionClosedByGame, LeftResult.PlayerLeft, 10, default),
            };

            var winners = w3GFileReader.GetWinners(winnerEvents, players, 1).ToList();
            Assert.AreEqual(2, winners[0].PlayerId);
            Assert.AreEqual(1, winners.Count);
        }

    }
}