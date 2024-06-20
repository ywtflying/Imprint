using System;
using System.Runtime.CompilerServices;

namespace WestLakeShape.Motion
{
    public class UnexpectedValueException : Exception
    {
        public UnexpectedValueException(
            object value,
            [CallerFilePath]
            string filePath = null,
            [CallerLineNumber]
            int lineNo = 0,
            [CallerMemberName]
            string callerMember = null)
            : base($"未期望的值: {value}, {callerMember}({filePath}:{lineNo})")
        {

        }
    }
}
