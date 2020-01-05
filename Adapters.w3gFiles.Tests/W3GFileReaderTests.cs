using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Adapters.w3gFiles.Tests
{
    public class W3GFileReaderTests
    {
        [Test]
        public async Task TestReadExpansionType()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(ExpansionType.TheFrozenThrone, game.ExpansionType);
        }

        [Test]
        public async Task TestReadVersion()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual("1.29.6060", game.Version.AsString);
        }

        [Test]
        public async Task TestReadMultiplayerFlag()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(PlayerMode.MultiPlayer, game.PlayerMode);
        }

        [Test]
        public async Task TestReadTime()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(new TimeSpan(0, 0, 12, 14, 325), game.GameTime);
        }

        [Test]
        public async Task TestReadPlayer()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual("modmoto", game.Host.Name);
            Assert.AreEqual(1, game.Host.PlayerId);
            Assert.AreEqual(GameMode.Ladder, game.GameType);
            Assert.AreEqual(Race.NightElve, game.Host.Race);
        }

        [Test]
        public async Task TestMapReadPlayer()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual("BNet", game.Map.GameName);
            Assert.AreEqual("Maps/FrozenThrone/(4)TwistedMeadows.w3x", game.Map.MapPath);
            Assert.AreEqual("(4)TwistedMeadows", game.Map.MapName);
        }

        [Test]
        public async Task TestGetPlayers()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual("modmoto", game.Players.ToList()[0].Name);
            Assert.AreEqual(1, game.Players.ToList()[0].PlayerId);
            Assert.AreEqual("Jason.Z", game.Players.ToList()[1].Name);
            Assert.AreEqual(2, game.Players.ToList()[1].PlayerId);
            Assert.AreEqual(Race.NightElve, game.Players.ToList()[0].Race);
            Assert.AreEqual(Race.Orc, game.Players.ToList()[1].Race);
            Assert.True(game.Players.ToList()[0].IsReplayOwner);
            Assert.False(game.Players.ToList()[0].IsAdditionalPlayer);
            Assert.True(game.Players.ToList()[1].IsAdditionalPlayer);
            Assert.False(game.Players.ToList()[1].IsReplayOwner);
        }

        [Test]
        public async Task TestGetGameSlots()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            Assert.AreEqual(1, game.GameSlots.ToList()[0].PlayerId);
            Assert.AreEqual(Race.NightElve, game.GameSlots.ToList()[0].Race);
            Assert.AreEqual(SlotUsage.Used, game.GameSlots.ToList()[0].SlotUsage);
            Assert.AreEqual(2, game.GameSlots.ToList()[1].PlayerId);
            Assert.AreEqual(Race.Orc, game.GameSlots.ToList()[1].Race);
            Assert.AreEqual(SlotUsage.Used, game.GameSlots.ToList()[1].SlotUsage);
            Assert.AreEqual(SlotUsage.Empty, game.GameSlots.ToList()[2].SlotUsage);
            Assert.AreEqual(SlotUsage.Empty, game.GameSlots.ToList()[3].SlotUsage);
        }

        [Test]
        public async Task TestGetChatMessages()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            var chatMessages = game.ChatMessages.ToList();
            Assert.AreEqual(1, chatMessages[0].PlayerId);
            Assert.AreEqual(2, chatMessages[1].PlayerId);
            Assert.AreEqual("gl hf", chatMessages[0].Message);
            Assert.AreEqual("gl", chatMessages[1].Message);
            Assert.AreEqual("gg", chatMessages[2].Message);
            Assert.AreEqual(1, chatMessages[2].PlayerId);
        }

        [Test]
        public async Task TestGetWinners()
        {
            var w3GFileReader = new W3GFileReader("TestGames/1_29.w3g");
            var game = await w3GFileReader.ReadAsync();
            var players = game.Winners.ToList();
            Assert.AreEqual(2, players[0].PlayerId);
            Assert.AreEqual("Jason.Z", players[0].Name);
            Assert.AreEqual(1, players.Count);
        }
    }
}