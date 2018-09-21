using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static string UntilNull(this IEnumerable<byte> data)
        {
            var array = data.TakeWhile(b => b != 0).ToArray();
            return Encoding.UTF8.GetString(array);
        }

        public static bool GetBit(this byte b, int bitNumber)
        {
            var bit = (b & (1 << bitNumber)) != 0;
            return bit;
        }
    }
}