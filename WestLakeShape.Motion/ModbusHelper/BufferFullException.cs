using System;

namespace WestLakeShape.Motion
{
    public class BufferFullException : Exception
    {
        public BufferFullException()
            : base("缓冲区满了")
        {
        }
    }
}
