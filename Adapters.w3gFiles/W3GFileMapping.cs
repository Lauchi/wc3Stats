using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Adapters.w3gFiles
{
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

        public GameOwnerHeader GetGameMetaData()
        {
            var contentSize = _fileBytesContent.Word(0x0000);
            var zippedContent = _fileBytesContent.Skip(0x0008).Take(contentSize).ToArray();
            var bytesDecompressed = DecompressZLibRaw(zippedContent).ToList();

            var playerRecord = GetPlayerRecord(bytesDecompressed);
            var map = GetTheMap(bytesDecompressed, playerRecord.Name.Length + 7);

            return new GameOwnerHeader(
                new GameOwner(playerRecord.Name, playerRecord.PlayerId, playerRecord.Race, playerRecord.GameType),
                playerRecord.GameType, map.Map, map.Players);
        }

        private MapAndPlayers GetTheMap(List<byte> bytesDecompressed, int index)
        {
            var findIndex = bytesDecompressed.FindIndex(index, b => b == 0x00);
            var enumerable = bytesDecompressed.Skip(findIndex + 3).ToArray();
            var gameName = enumerable.UntilNull();
            var compressedMapHeader = bytesDecompressed.Skip(findIndex + 5 + gameName.Length).ToArray();
            var encodedString = compressedMapHeader.TakeWhile(b => b != '\0').ToArray();
            var encodedStringLength = encodedString.Length;
            var mapName = GetMapName(encodedString);
            var startOfPlayerCount = encodedStringLength + 6 + findIndex + gameName.Length;
            var playerCount = GetPlayerCount(bytesDecompressed.Skip(startOfPlayerCount).ToList());
            var startOfPlayerList = startOfPlayerCount + 12;

            var s = string.Join("", bytesDecompressed.Select(b => (char)b));
            var players = GetPlayers(bytesDecompressed.Take(startOfPlayerList).ToList(), playerCount);

            return new MapAndPlayers(new Map(gameName, mapName), players);
        }

        private IEnumerable<Player> GetPlayers(List<byte> bytesDecompressed, uint playerCount)
        {
            var offset = 0;
            for (var i = 0; i < playerCount; i++)
            {
                var player = GetPlayerRecord(bytesDecompressed);
                var playerLength = player.GameType == GameMode.Ladder ? 11 : 4;
                offset += playerLength + player.Name.Length + 4;
                bytesDecompressed = bytesDecompressed.Skip(offset).ToList();
                yield return player;
            }
        }

        private Player GetPlayerRecord(List<byte> bytesDecompressed)
        {
            var playerId = BitConverter.ToUInt32(new byte[] {bytesDecompressed[5], 0, 0, 0}, 0);
            var playerData = bytesDecompressed.Skip(6).ToArray();
            var name = playerData.UntilNull();
            var gameType = GameType(bytesDecompressed, name.Length + 7);

            var race = GetRace(gameType, bytesDecompressed, name.Length + 7);
            return new Player(name, playerId, race, gameType);
        }

        private uint GetPlayerCount(List<byte> bytesDecompressed)
        {
            var uInt32 = BitConverter.ToUInt32(new byte[] { bytesDecompressed[0], 0, 0, 0}, 0);
            return uInt32;
        }

        private string GetMapName(byte[] encodedString)
        {
            var decodedString = new List<char>();
            var mask = encodedString[0];
            var positionInStream = 0;

            foreach (var encodedCharacter in encodedString)
            {
                if (IsChecksumByte(positionInStream))
                {
                    mask = encodedCharacter;
                }
                else
                {
                    if (mask.BitIsSet(positionInStream % 8))
                    {
                        var inner = encodedCharacter;
                        var decompressed = inner - 1;
                        decodedString.Add((char) decompressed);
                    }
                    else
                    {
                        decodedString.Add((char) encodedCharacter);
                    }
                }

                positionInStream++;
            }

            var decompressedJoinesName = string.Join("", decodedString);
            var mapName = string.Join("", decompressedJoinesName.Split('\0')[6].Skip(4));
            return mapName;
        }

        private static bool IsChecksumByte(int positionInStream)
        {
            return positionInStream % 8 == 0;
        }

        private static GameMode GameType(List<byte> bytesDecompressed, int gameTypeIndex)
        {
            var gameTypeByte = bytesDecompressed[gameTypeIndex];
            var gameType = GameMode.Undefined;
            switch (gameTypeByte)
            {
                case 0x01:
                    gameType = GameMode.Custom;
                    break;
                case 0x08:
                    gameType = GameMode.Ladder;
                    break;
            }

            return gameType;
        }

        private static Race GetRace(GameMode gameType, List<byte> bytesDecompressed, int gameTypeIndex)
        {
            var race = Race.Undefined;
            switch (gameType)
            {
                case GameMode.Custom:
                    race = Race.CustomGame;
                    break;
                case GameMode.Ladder:
                {
                    var array = bytesDecompressed.Skip(gameTypeIndex + 5).Take(4).ToArray();
                    var dWord = array.DWord(0);
                    switch (dWord)
                    {
                        case 0x01:
                            race = Race.Human;
                            break;
                        case 0x02:
                            race = Race.Orc;
                            break;
                        case 0x04:
                            race = Race.NightElve;
                            break;
                        case 0x08:
                            race = Race.Undead;
                            break;
                        case 0x20:
                            race = Race.Random;
                            break;
                        case 0x40:
                            race = Race.Fixed;
                            break;
                    }

                    break;
                }
            }

            return race;
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

    internal class MapAndPlayers
    {
        public MapAndPlayers(Map map, IEnumerable<Player> players)
        {
            Map = map;
            Players = players;
        }

        public Map Map { get; }
        public IEnumerable<Player> Players { get; }
    }
}