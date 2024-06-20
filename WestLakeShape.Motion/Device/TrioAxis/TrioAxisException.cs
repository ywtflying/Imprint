using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WestLakeShape.Motion.Device
{
    public class TrioApiException : Exception
    {
        public TrioApiException()
        {
            //[CallerMemberName] string memeberName = "",
            //[CallerFilePath] string sourceFilePath = "",
            //[CallerLineNumber] int sourceLineNumber = 0
            //ReturnCode = returnCode;
            //ReturnMessage = $"使用者：{memeberName}；文件路径{sourceFilePath}；行数{sourceLineNumber}";
        }

        public bool ReturnCode { get; private set; }

        public string ReturnMessage { get; private set; }

        public override string Message
        {
            get
            {
                return $"Trio接口调用失败: {ReturnCode}.{ReturnMessage}";
            }
        }

        public static void Check(bool rt)
        {
            if (!rt)
                throw new TrioApiException();
        }
    }
}
