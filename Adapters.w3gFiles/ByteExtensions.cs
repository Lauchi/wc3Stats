using System;

namespace Adapters.w3gFiles
{
    public static class ByteExtensions
    {
        public static uint DWord(this byte[] bytes, int offset)
        {
            return BitConverter.ToUInt32(bytes, offset);
        }

        public static ushort Word(this byte[] bytes, int offset)
        {
            return BitConverter.ToUInt16(bytes, offset);
        }
    }
}