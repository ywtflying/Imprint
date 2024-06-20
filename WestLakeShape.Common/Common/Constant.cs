using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WestLakeShape.Common
{
    public static class Constants
    {
        private static Encoding _encoding;

        public const int DelayInterval = 1;

        public const string EncodingName = "GBK";
        public static string ConfigRootFolder = @"D:\NanoImprinterConfig\";
        public static Encoding Encoding
        {
            get
            {
                if (_encoding == null)
                    _encoding = Encoding.GetEncoding(EncodingName);

                return _encoding;
            }
        }

    }
}
