using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Adapters.w3gFiles
{
    public class W3GFileMapping
    {
        private byte[] _contentDecompressed;
        private byte[] _fileBytes;
        private byte[] _fileBytesContent;
        private byte[] _fileBytesHeader;
        private byte[] _gameActionBytes;

        public ICollection<ChatMessage> Messages { get; } = new Collection<ChatMessage>();

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

            switch (majorVersion)
            {
                case 29: return GameVersion.v1_29;
                case 10031: return GameVersion.v1_31_3;
                default: return GameVersion.unsupported;
            }
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
            _contentDecompressed = Decompress(_fileBytesContent).ToArray();
        }

        public TimeSpan GetPlayedTime()
        {
            var milliseconds = _fileBytesHeader.DWord(0x0C);
            var timeSpan = new TimeSpan(0, 0, 0, 0, (int) milliseconds);
            return timeSpan;
        }

        public GameHeader GetGameMetaData()
        {
            var playerRecord = GetPlayerRecord(_contentDecompressed.Skip(4).ToList());

            var findIndex = _contentDecompressed.ToList().FindIndex(playerRecord.Name.Length + 7, b => b == 0x00);
            var gameNameArray = _contentDecompressed.Skip(findIndex + 3).ToArray();
            var gameName = gameNameArray.UntilNull();
            var compressedMapHeader = _contentDecompressed.Skip(findIndex + 5 + gameName.Length).ToArray();
            var encodedString = compressedMapHeader.TakeWhile(b => b != '\0').ToArray();
            var encodedStringLength = encodedString.Length;
            var mapName = GetMapName(encodedString);
            var startOfPlayerCount = encodedStringLength + 6 + findIndex + gameName.Length;
            var playerCount = GetPlayerCount(_contentDecompressed.Skip(startOfPlayerCount).ToList());
            var startOfPlayerList = startOfPlayerCount + 12;

            var players = GetPlayers(_contentDecompressed.Skip(startOfPlayerList).ToList(), playerCount).ToList();

            var playerListSize = 0;
            foreach (var player in players)
            {
                var playerLength = player.GameType == GameMode.Ladder ? 11 : 4;
                playerListSize += playerLength + player.Name.Length + 4;
            }

            var startOfGameStartSegment = startOfPlayerList + playerListSize;
            var gameStartSegment = _contentDecompressed.Skip(startOfGameStartSegment + 1).ToList();
            var gameSlots = GetGameSlots(gameStartSegment);


            var startOfActionIndex = startOfGameStartSegment + gameSlots.Count * 9 + 11;
            _gameActionBytes = _contentDecompressed.Skip(startOfActionIndex + 15).ToArray();
            var map = new MapAndPlayers(new Map(gameName, mapName), players, gameSlots);

            return new GameHeader(
                new GameOwner(playerRecord.Name, playerRecord.PlayerId, playerRecord.Race, playerRecord.GameType,
                    playerRecord.IsAdditionalPlayer),
                playerRecord.GameType, map.Map, map.Players, map.GameSlots);
        }

        private IEnumerable<byte> Decompress(byte[] fileBytesContent)
        {
            var bytes = new List<byte>();
            while (fileBytesContent.Length != 0)
            {
                var contentSize = fileBytesContent.Word(0x00);
                var zippedContent = fileBytesContent.Skip(0x08).Take(contentSize);
                var decompressed = DecompressZLibRaw(zippedContent.ToArray());
                bytes.AddRange(decompressed);
                fileBytesContent = fileBytesContent.Skip(contentSize + 0x08).ToArray();
            }

            return bytes;
        }

        private List<GameSlot> GetGameSlots(List<byte> gameStartSegment)
        {
            var numberOfPlayerRecords = (int) gameStartSegment[3];
            var startOfRecords = gameStartSegment.Skip(4).ToArray();

            var records = ParseGameSlots(numberOfPlayerRecords, startOfRecords).ToList();
            return records;
        }

        private IEnumerable<GameSlot> ParseGameSlots(int numberOfPlayerRecords, byte[] startOfRecords)
        {
            for (var i = 0; i < numberOfPlayerRecords; i++)
            {
                var playerId = (int) startOfRecords[i * 9 + 0];
                var slotUsage = ToSlotUsage(startOfRecords[i * 9 + 2]);
                var isHuman = startOfRecords[i * 9 + 3] == 0x00;
                var teamNumber = (int) startOfRecords[i * 9 + 4];
                var race = ToRace(startOfRecords[i * 9 + 6]);
                yield return new GameSlot(playerId, slotUsage, isHuman, teamNumber, race);
            }
        }

        private Race ToRace(byte startOfRecord)
        {
            switch (startOfRecord)
            {
                case 0X01: return Race.Human;
                case 0X02: return Race.Orc;
                case 0X04: return Race.NightElve;
                case 0X08: return Race.Undead;
                case 0X20: return Race.Random;
                case 0X40: return Race.Fixed;
                default: return Race.Unknown;
            }
        }

        private SlotUsage ToSlotUsage(byte startOfRecord)
        {
            switch (startOfRecord)
            {
                case 0X00: return SlotUsage.Empty;
                case 0X01: return SlotUsage.Closed;
                case 0X02: return SlotUsage.Used;
                default: return SlotUsage.Unknown;
            }
        }

        private IEnumerable<Player> GetPlayers(List<byte> bytesDecompressed, uint playerCount)
        {
            var offset = 0;
            for (var i = 0; i < playerCount - 1; i++)
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
            var isAdditionalPlayer = bytesDecompressed[0] == 0x16;
            var playerId = BitConverter.ToUInt32(new byte[] {bytesDecompressed[1], 0, 0, 0}, 0);
            var playerData = bytesDecompressed.Skip(2).ToArray();
            var name = playerData.UntilNull();
            var gameType = GameType(bytesDecompressed, name.Length + 3);

            var race = GetRace(gameType, bytesDecompressed, name.Length + 3);
            return new Player(name, playerId, race, gameType, isAdditionalPlayer);
        }

        private uint GetPlayerCount(List<byte> bytesDecompressed)
        {
            var uInt32 = BitConverter.ToUInt32(new byte[] {bytesDecompressed[0], 0, 0, 0}, 0);
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
            var mapName = decompressedJoinesName.Split('/').First(s => s.Contains(".w3x"));
            var mapNameForReal = mapName.Split(new [] { ".w3x" }, StringSplitOptions.None);
            return mapNameForReal.First();
        }

        private static bool IsChecksumByte(int positionInStream)
        {
            return positionInStream % 8 == 0;
        }

        private static GameMode GameType(List<byte> bytesDecompressed, int gameTypeIndex)
        {
            var gameTypeByte = bytesDecompressed[gameTypeIndex];
            switch (gameTypeByte)
            {
                case 0x01: return GameMode.Custom;
                case 0x08: return GameMode.Ladder;
                default: return GameMode.Undefined;
            }
        }

        private static Race GetRace(GameMode gameType, List<byte> bytesDecompressed, int gameTypeIndex)
        {
            switch (gameType)
            {
                case GameMode.Custom: return Race.CustomGame;
                case GameMode.Ladder:
                {
                    var array = bytesDecompressed.Skip(gameTypeIndex + 5).Take(4).ToArray();
                    var dWord = array.DWord();
                    switch (dWord)
                    {
                        case 0x01: return Race.Human;
                        case 0x02: return Race.Orc;
                        case 0x04: return Race.NightElve;
                        case 0x08: return Race.Undead;
                        case 0x20: return Race.Random;
                        case 0x40: return Race.Fixed;
                        default: return Race.Undefined;
                    }
                }
                default: return Race.Undefined;
            }
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

        public IEnumerable<IGameAction> GetActions()
        {
            var offset = 0;
            while (_gameActionBytes[offset] != 0)
                switch (_gameActionBytes[offset])
                {
                    case GameActions.PlayerActionNew:
                    case GameActions.PlayerActionOld:
                    {
                        var bytesForActions = new[] {_gameActionBytes[offset + 1], _gameActionBytes[offset + 2]}.Word();
                        offset += 3 + bytesForActions;
                        break;
                    }
                    case GameActions.ChatMessage:
                    {
                        var playerId = (int) _gameActionBytes[offset + 1];
                        var messageCount = new[] {_gameActionBytes[offset + 2], _gameActionBytes[offset + 3]}.Word();
                        var message = _gameActionBytes.UntilNull(offset + 9);
                        offset += 4 + messageCount;
                        yield return new ChatMessage(playerId, message);
                        break;
                    }
                    case GameActions.LeftGame:
                    {
                        var reason = _gameActionBytes[offset + 1];
                        var playerId = (int) _gameActionBytes[offset + 5];
                        var result = _gameActionBytes[offset + 6];
                        var unknownWinFlag = _gameActionBytes.DWord(offset + 10);
                        offset += 14;

                        yield return new PlayerLeft(playerId, reason, result, unknownWinFlag);
                        break;
                    }
                    // all unecessary stuff below
                    case 0x22:
                    {
                        offset += 6;
                        break;
                    }
                    case 0x23:
                    {
                        offset += 11;
                        break;
                    }
                    case 0x2F:
                    {
                        offset += 9;
                        break;
                    }
                    default:
                        Console.WriteLine(
                            $"Could not parse Actiontype0x{BitConverter.ToString(new[] {_gameActionBytes[offset]})} on Location {offset}, will abort parsing Actions, actionlist is most likely not comprehensive");
                        yield break;
                }
        }
    }
}