using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Adapters.w3gFiles
{
    public class W3GFileReader
    {
        private readonly W3GFileMapping _mapping;

        public W3GFileReader(W3GFileMapping mapping)
        {
            _mapping = mapping;
        }
        public Wc3Game Read(string filePath)
        {

            byte[] fileBytes = File.ReadAllBytes(filePath);
            _mapping.SetBytes(fileBytes);

            var expansionType = _mapping.GetExpansionType();
            var version = _mapping.GetVersion();
            var isMultiPlayer = _mapping.GetIsMultiPlayer();
            var time = _mapping.GetPlayedTime();

            return new Wc3Game(expansionType, version, isMultiPlayer, time);
        }
    }

    public class W3GFileMapping
    {
        private byte[] _fileBytes;
        private byte[] _fileBytesHeader;

        public ExpansionType GetExpansionType()
        {
            var expansionType = Encoding.UTF8.GetString(new [] { _fileBytes[51], _fileBytes[50] , _fileBytes[49], _fileBytes[48] });
            switch (expansionType)
            {
                    case "W3XP": return ExpansionType.TheFrozenThrone;
                    case "WAR3": return ExpansionType.ReignOfChaos;
                   default: throw new ArgumentException($"No correct Game Type found: {expansionType}");
            }
        }

        public GameVersion GetVersion()
        {
            var startIndex = 0x0004;
            Console.WriteLine(startIndex);
            var majorVersion = BitConverter.ToUInt32(_fileBytesHeader, startIndex);
            var buildVersion = BitConverter.ToUInt16(_fileBytesHeader, 0x0008);

            return new GameVersion(majorVersion, buildVersion);
        }

        public PlayerMode GetIsMultiPlayer()
        {
            var uInt32 = BitConverter.ToUInt16(_fileBytesHeader, 0x000A);
            switch (uInt32)
            {
                    case 0x0 : return PlayerMode.SinglePlayer;
                    case 0x8000 : return PlayerMode.MultiPlayer;
                    default: throw new ArgumentException($"No correct MultiPlayerMode found: {uInt32}");
            }
        }

        public void SetBytes(byte[] fileBytes)
        {
            _fileBytes = fileBytes;
            _fileBytesHeader = fileBytes.Skip(48).ToArray();
        }

        public TimeSpan GetPlayedTime()
        {
            var milliseconds = BitConverter.ToUInt32(_fileBytesHeader, 0x000C);
            var timeSpan = new TimeSpan(0,0,0,0,(int) milliseconds);
            return timeSpan;
        }
    }

    public enum PlayerMode
    {
        MultiPlayer, SinglePlayer
    }

    public class GameVersion
    {
        public uint MajorVersion { get; }
        public ushort BuildVersion { get; }
        public string AsString => $"1.{MajorVersion}.{BuildVersion}";

        public GameVersion(uint majorVersion, ushort buildVersion)
        {
            MajorVersion = majorVersion;
            BuildVersion = buildVersion;
        }
    }

    public enum ExpansionType
    {
        TheFrozenThrone, ReignOfChaos
    }

    public class Wc3Game
    {
        public ExpansionType ExpansionType { get; }
        public GameVersion Version { get; }
        public PlayerMode PlayerMode { get; }
        public TimeSpan GameTime { get; }

        public Wc3Game(ExpansionType expansionType, GameVersion version, PlayerMode playerMode, TimeSpan gameTime)
        {
            ExpansionType = expansionType;
            Version = version;
            PlayerMode = playerMode;
            GameTime = gameTime;
        }

        public IEnumerable<Player> Players { get; }
    }

    public class Player
    {
        public string Name { get; set; }
    }
}