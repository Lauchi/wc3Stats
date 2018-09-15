namespace Adapters.w3gFiles
{
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
}