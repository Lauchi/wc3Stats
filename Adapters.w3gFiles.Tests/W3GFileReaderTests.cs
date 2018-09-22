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
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = await w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual(ExpansionType.TheFrozenThrone, game.ExpansionType);
        }

        [Test]
        public async Task TestReadVersion()
        {
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = await w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual("1.29.6060", game.Version.AsString);
        }

        [Test]
        public async Task TestReadMultiplayerFlag()
        {
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = await w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual(PlayerMode.MultiPlayer, game.PlayerMode);
        }

        [Test]
        public async Task TestReadTime()
        {
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = await w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual(new TimeSpan(0, 0, 12, 14, 325), game.GameTime);
        }

        [Test]
        public async Task TestReadPlayer()
        {
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = await w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual("modmoto", game.Host.Name);
            Assert.AreEqual(1, game.Host.PlayerId);
            Assert.AreEqual(GameMode.Ladder, game.GameType);
            Assert.AreEqual(Race.NightElve, game.Host.Race);
        }

        [Test]
        public async Task TestMapReadPlayer()
        {
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = await w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual("BNet", game.Map.GameName);
            Assert.AreEqual("Maps/FrozenThrone/(4)TwistedMeadows.w3x", game.Map.MapPath);
            Assert.AreEqual("(4)TwistedMeadows", game.Map.MapName);
        }

        [Test]
        public async Task     TestGetPlayers()
        {
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = await w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual("modmoto", game.Players.ToList()[0].Name);
            Assert.AreEqual("irgendwer", game.Players.ToList()[1].Name);
            Assert.AreEqual(Race.NightElve, game.Players.ToList()[0].Race);
            Assert.AreEqual(Race.Orc, game.Players.ToList()[1].Race);
        }
    }
}