using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Adapters.w3gFiles.Tests
{
    public class W3GFileReaderTests
    {
        [Test]
        [TestCase("TestGames/1_29.w3g")]
        [TestCase("TestGames/1_31.3_custom.w3g")]
        [TestCase("TestGames/1_31.3_ladder.w3g")]
        public async Task TestReadExpansionType(string replay)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(ExpansionType.TheFrozenThrone, game.ExpansionType);
        }

        [Test]
        [TestCase("TestGames/1_29.w3g", GameVersion.v1_29)]
        [TestCase("TestGames/1_31.3_custom.w3g", GameVersion.v1_31_3)]
        [TestCase("TestGames/1_31.3_ladder.w3g", GameVersion.v1_31_3)]
        public async Task TestReadVersion(string replay, GameVersion version)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(version, game.Version);
        }

        [Test]
        [TestCase("TestGames/1_29.w3g")]
        [TestCase("TestGames/1_31.3_custom.w3g")]
        [TestCase("TestGames/1_31.3_ladder.w3g")]
        public async Task TestReadMultiplayerFlag(string replay)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(PlayerMode.MultiPlayer, game.PlayerMode);
        }

        [Test]
        [TestCase("TestGames/1_29.w3g", 12, 14, 325)]
        [TestCase("TestGames/1_31.3_custom.w3g", 27, 36, 375)]
        [TestCase("TestGames/1_31.3_ladder.w3g", 28, 32, 175)]
        public async Task TestReadTime(string replay, int minutes, int seconds, int milliseconds)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(new TimeSpan(0, 0, minutes, seconds, milliseconds), game.GameTime);
        }

        [Test]
        [TestCase("TestGames/1_29.w3g", "modmoto", Race.NightElve, GameMode.Ladder)]
        [TestCase("TestGames/1_31.3_custom.w3g", "modmoto", Race.NightElve, GameMode.Custom)]
        [TestCase("TestGames/1_31.3_ladder.w3g", "thementalist", Race.Orc, GameMode.Ladder)]
        public async Task TestReadPlayer(string replay, string hostName, Race race, GameMode gameType)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(hostName, game.Host.Name);
            Assert.AreEqual(1, game.Host.PlayerId);
            Assert.AreEqual(gameType, game.GameType);
            Assert.AreEqual(race, game.Host.Race);
        }

        [Test]
        [TestCase("TestGames/1_29.w3g", "(4)TwistedMeadows", "BNet")]
        [TestCase("TestGames/1_31.3_custom.w3g", "(4)TwistedMeadows", "btv4b")]
        [TestCase("TestGames/1_31.3_ladder.w3g", "(2)AncientIsles", "BNet")]
        public async Task TestMapAndGameName(string replay, string mapName, string gameName)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(gameName, game.Map.GameName);
            Assert.AreEqual(mapName, game.Map.MapName);
        }

        [Test]
        [TestCase("TestGames/1_29.w3g", "modmoto", Race.NightElve, "Jason.Z", Race.Orc)]
        [TestCase("TestGames/1_31.3_custom.w3g", "modmoto", Race.NightElve, "xAluCarDx", Race.Orc)]
        [TestCase("TestGames/1_31.3_ladder.w3g", "thementalist", Race.Orc, "modmoto", Race.NightElve)]
        public async Task TestGetPlayers(string replay, string player1, Race race1, string player2, Race race2)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(player1, game.Players.ToList()[0].Name);
            Assert.AreEqual(1, game.Players.ToList()[0].PlayerId);
            Assert.AreEqual(player2, game.Players.ToList()[1].Name);
            Assert.AreEqual(2, game.Players.ToList()[1].PlayerId);
            Assert.AreEqual(race1, game.Players.ToList()[0].Race);
            Assert.AreEqual(race2, game.Players.ToList()[1].Race);
            Assert.True(game.Players.ToList()[0].IsReplayOwner);
            Assert.False(game.Players.ToList()[0].IsAdditionalPlayer);
            Assert.True(game.Players.ToList()[1].IsAdditionalPlayer);
            Assert.False(game.Players.ToList()[1].IsReplayOwner);
        }

        [Test]
        [TestCase("TestGames/1_29.w3g", Race.NightElve, Race.Orc)]
        [TestCase("TestGames/1_31.3_custom.w3g", Race.NightElve, Race.Orc)]
        [TestCase("TestGames/1_31.3_ladder.w3g", Race.Orc, Race.NightElve)]
        public async Task TestGetGameSlots(string replay, Race race1, Race race2)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(1, game.GameSlots.ToList()[0].PlayerId);
            Assert.AreEqual(race1, game.GameSlots.ToList()[0].Race);
            Assert.AreEqual(SlotUsage.Used, game.GameSlots.ToList()[0].SlotUsage);
            Assert.AreEqual(2, game.GameSlots.ToList()[1].PlayerId);
            Assert.AreEqual(race2, game.GameSlots.ToList()[1].Race);
            Assert.AreEqual(SlotUsage.Used, game.GameSlots.ToList()[1].SlotUsage);
            // Assert.AreEqual(SlotUsage.Empty, game.GameSlots.ToList()[2].SlotUsage);
            // Assert.AreEqual(SlotUsage.Empty, game.GameSlots.ToList()[3].SlotUsage);
        }

        [Test]
        [TestCase("TestGames/1_29.w3g")]
        [TestCase("TestGames/1_31.3_custom.w3g")]
        [TestCase("TestGames/1_31.3_ladder.w3g")]
        public async Task TestGetChatMessages(string replay)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            var chatMessages = game.ChatMessages.ToList();
            Assert.AreEqual("gl hf", chatMessages[0].Message);
        }

        [Test]
        [TestCase("TestGames/1_29.w3g", "Jason.Z")]
        [TestCase("TestGames/1_31.3_custom.w3g", "Jason.Z")]
        [TestCase("TestGames/1_31.3_ladder.w3g", "modmoto")]
        public async Task TestGetWinners(string replay, string playerName)
        {
            var w3GFileReader = new W3GFileReader(replay);
            var game = await w3GFileReader.ReadAsync();
            var players = game.Winners.ToList();
            Assert.AreEqual(2, players[0].PlayerId);
            Assert.AreEqual(playerName, players[0].Name);
            Assert.AreEqual(1, players.Count);
        }
    }
}