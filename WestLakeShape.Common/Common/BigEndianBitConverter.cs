using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WestLakeShape.Common
{
    public static class BigEndianBitConverter
    {
        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return unchecked((uint)(FromBytes(value, startIndex, 4)));
        }

        public static long FromBytes(byte[] buffer, int startIndex, int bytesToConvert)
        {
            long ret = 0;
            for (int i = 0; i < bytesToConvert; i++)
            {
                ret = unchecked((ret << 8) | buffer[startIndex + i]);
            }
            return ret;
        }
    }
}
