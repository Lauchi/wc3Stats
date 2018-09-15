using NUnit.Framework;

namespace Adapters.w3gFiles.Tests
{
    public class W3GFileReaderTests
    {
        [Test]
        public void TestReadExpansionType()
        {
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual(ExpansionType.TheFrozenThrone, game.ExpansionType);
        }

        [Test]
        public void TestReadVersion()
        {
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual("1.29.6060", game.Version.AsString);
        }

        [Test]
        public void TestReadMultiplayerFlag()
        {
            var w3GFileReader = new W3GFileReader(new W3GFileMapping());
            var game = w3GFileReader.Read("TestGames/1_29.w3g");
            Assert.AreEqual(PlayerMode.MultiPlayer, game.PlayerMode);
        }
    }
}