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

            var playerId = BitConverter.ToUInt32(new byte[] {bytesDecompressed[5], 0, 0, 0}, 0);
            var playerData = bytesDecompressed.Skip(6).ToArray();
            var name = playerData.UntilNull();

            var gameType = GameType(bytesDecompressed, name.Length + 7);
            var race = GetRace(gameType, bytesDecompressed, name.Length + 7);
            //var map = GetTheMap(bytesDecompressed, name.Length + 7);



            return new GameOwnerHeader(new GameOwner(name, playerId, race), gameType);
        }

        private Map GetTheMap(List<byte> bytesDecompressed, int index)
        {
            var findIndex = bytesDecompressed.FindIndex(index, b => b == 0x00);
            var enumerable = bytesDecompressed.Skip(findIndex + 3).ToArray();
            var gameName = enumerable.UntilNull();
            var enumerable2 = bytesDecompressed.Skip(findIndex + 5 + gameName.Length).ToArray();
            var decomp = DecompWeirdThing(enumerable2);

            return new Map(gameName);
        }

        private string DecompWeirdThing(byte[] encodedString)
        {
            var untilNull = encodedString.UntilNull();
            byte bitOrder = (byte) untilNull[0];

            var chars = new List<string>();

            var i = 6;
            foreach (var c in untilNull.Skip(1).Take(7))
            {
                var charachterBYte = (byte) c;
                var isSet = bitOrder.GetBit(i);
                if (isSet)
                {
                    chars.Add(Encoding.UTF8.GetString(new [] { charachterBYte }));
                }
                else
                {
                    var i1 = (byte) (charachterBYte - 1);
                    chars.Add(Encoding.UTF8.GetString(new [] { i1 }));
                }

                i--;
            }

//            var charArray = untilNull.ToCharArray();
//            var bytes1 = Encoding.UTF8.GetBytes(untilNull);
//            var chars = new List<string>();
//
//            foreach (var b in bytes1)
//            {
//                var startsWithOne = b & 0x80;
//                if (startsWithOne == 0x80)
//                {
//                    var item = Encoding.UTF8.GetString(new [] { b });
//                    chars.Add(item);
//                }
//                else
//                {
//                    var b1 = Convert.ToByte(b);
//                    var int32 = BitConverter.ToInt32(new [] { b1, Byte.MinValue, Byte.MinValue, Byte.MinValue }, 0);
//                    var value = int32 - 1;
//                    var bytes = BitConverter.GetBytes(value);
//                    chars.Add(Encoding.UTF8.GetString(bytes));
//                }
//            }

            var word = string.Join("", chars);
            return word;
        }

        private static GameMode GameType(List<byte> bytesDecompressed, int gameTypeIndex)
        {
            var gameTypeByte = bytesDecompressed[gameTypeIndex];
            GameMode gameType = GameMode.Undefined;
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

    public class Map
    {
        public string GameName { get; }

        public Map(string gameName)
        {
            GameName = gameName;
        }
    }

    public class GameOwnerHeader
    {
        public GameOwner GameOwner { get; }
        public GameMode GameType { get; }

        public GameOwnerHeader(GameOwner gameOwner, GameMode gameType)
        {
            GameOwner = gameOwner;
            GameType = gameType;
        }
    }
}