using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapters.w3gFiles
{
    public class W3GFileReader
    {
        private readonly W3GFileMapping _mapping;

        public W3GFileReader(W3GFileMapping mapping)
        {
            _mapping = mapping;
        }

        public async Task<Wc3Game> Read(string filePath)
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            _mapping.SetBytes(fileBytes);

            var expansionType = _mapping.GetExpansionType();
            var version = _mapping.GetVersion();
            var isMultiPlayer = _mapping.GetIsMultiPlayer();
            var time = _mapping.GetPlayedTime();
            var players = _mapping.GetPlayers();

            return new Wc3Game(players, expansionType, version, isMultiPlayer, time);
        }
    }

    public class W3GFileMapping
    {
        private byte[] _fileBytes;
        private byte[] _fileBytesContent;
        private byte[] _fileBytesHeader;

        public ExpansionType GetExpansionType()
        {
            var expansionType = Encoding.UTF8.GetString(new[]
                {_fileBytes[51], _fileBytes[50], _fileBytes[49], _fileBytes[48]});
            switch (expansionType)
            {
                case "W3XP": return ExpansionType.TheFrozenThrone;
                case "WAR3": return ExpansionType.ReignOfChaos;
                default: throw new ArgumentException($"No correct Game Type found: {expansionType}");
            }
        }

        public GameVersion GetVersion()
        {
            var majorVersion = _fileBytesHeader.DWord(0x0004);
            var buildVersion = _fileBytesHeader.Word(0x0008);

            return new GameVersion(majorVersion, buildVersion);
        }

        public PlayerMode GetIsMultiPlayer()
        {
            var uInt32 = _fileBytesHeader.Word(0x000A);
            switch (uInt32)
            {
                case 0x0: return PlayerMode.SinglePlayer;
                case 0x8000: return PlayerMode.MultiPlayer;
                default: throw new ArgumentException($"No correct MultiPlayerMode found: {uInt32}");
            }
        }

        public void SetBytes(byte[] fileBytes)
        {
            _fileBytes = fileBytes;
            _fileBytesHeader = fileBytes.Skip(0x30).ToArray();
            _fileBytesContent = fileBytes.Skip(0x44).ToArray();
        }

        public TimeSpan GetPlayedTime()
        {
            var milliseconds = _fileBytesHeader.DWord(0x000C);
            var timeSpan = new TimeSpan(0, 0, 0, 0, (int) milliseconds);
            return timeSpan;
        }

        public IEnumerable<Player> GetPlayers()
        {
            var contentSize = _fileBytesContent.Word(0x0000);
            var zippedContent = _fileBytesContent.Skip(0x0008).Take(contentSize).ToArray();
            var decompress = DecompressZLibRaw(zippedContent).ToList();

            int playerOffset = 0;
            var playerId = BitConverter.ToUInt32(new byte[] {decompress[5], 0, 0, 0 }, 0);
            var playerData = decompress.Skip(6).ToArray();
            var name = playerData.UntilNull();
            var gameTypeIndex = name.Length + 7;
            var b = decompress[gameTypeIndex];
            GameMode gameType;
            switch (b)
            {
                case 0x01:
                    gameType = GameMode.Custom;
                    break;
                case 0x08:
                    gameType = GameMode.Ladder;
                    break;
                default: throw new ArgumentException("Not clear if leader or custom game");
            }

            Race race = Race.CustomGame;
            switch (gameType)
            {
                case GameMode.Custom:
                    race = Race.CustomGame;
                    playerOffset += playerOffset + gameTypeIndex + 1;
                    break;
                case GameMode.Ladder:
                {
                    var array = decompress.Skip(gameTypeIndex + 5).Take(4).ToArray();
                    var dWord = array.DWord(0);
                    switch (dWord)
                    {
                        case 0x01: race = Race.Human; break;
                        case 0x02: race = Race.Orc; break;
                        case 0x04: race = Race.NightElve; break;
                        case 0x08: race = Race.Undead; break;
                        case 0x20: race = Race.Random; break;
                        case 0x40: race = Race.Fixed; break;
                    }
                    playerOffset += playerOffset + gameTypeIndex + 8;
                    break;
                }
            }

            yield return new Player(name, playerId, gameType, race);
        }

        public static byte[] DecompressZLibRaw(byte[] bCompressed)
        {
            byte[] bHdr = {0x1F, 0x8b, 0x08, 0, 0, 0, 0, 0};

            using (var sOutput = new MemoryStream())
            using (var sCompressed = new MemoryStream())
            {
                sCompressed.Write(bHdr, 0, bHdr.Length);
                sCompressed.Write(bCompressed, 0, bCompressed.Length);
                sCompressed.Position = 0;
                using (var decomp = new GZipStream(sCompressed, CompressionMode.Decompress))
                {
                    decomp.CopyTo(sOutput);
                }

                return sOutput.ToArray();
            }
        }
    }

    public enum PlayerMode
    {
        MultiPlayer,
        SinglePlayer
    }

    public enum Race
    {
        CustomGame,
        Human,
        NightElve,
        Orc,
        Undead,
        Random,
        Fixed
    }

    public enum GameMode
    {
        Custom,
        Ladder
    }

    public class GameVersion
    {
        public GameVersion(uint majorVersion, ushort buildVersion)
        {
            MajorVersion = majorVersion;
            BuildVersion = buildVersion;
        }

        public uint MajorVersion { get; }
        public ushort BuildVersion { get; }
        public string AsString => $"1.{MajorVersion}.{BuildVersion}";
    }

    public enum ExpansionType
    {
        TheFrozenThrone,
        ReignOfChaos
    }

    public class Wc3Game
    {
        public Wc3Game(IEnumerable<Player> players, ExpansionType expansionType, GameVersion version,
            PlayerMode playerMode,
            TimeSpan gameTime)
        {
            Players = players;
            ExpansionType = expansionType;
            Version = version;
            PlayerMode = playerMode;
            GameTime = gameTime;
        }

        public ExpansionType ExpansionType { get; }
        public GameVersion Version { get; }
        public PlayerMode PlayerMode { get; }
        public TimeSpan GameTime { get; }
        public IEnumerable<Player> Players { get; }
    }

    public class Player
    {
        public Player(string name, uint playerId, GameMode gameType, Race race)
        {
            Name = name;
            PlayerId = playerId;
            GameType = gameType;
            Race = race;
        }

        public uint PlayerId { get; }
        public GameMode GameType { get; }
        public Race Race { get; }
        public string Name { get; }
    }
}