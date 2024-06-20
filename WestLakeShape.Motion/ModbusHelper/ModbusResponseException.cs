using System;

namespace WestLakeShape.Motion
{
    public class ModbusResponseException : Exception
    {
        public ModbusResponseException(byte exceptionCode)
            : base("Modbus Slave 返回异常代码：" + exceptionCode)
        {
            ExceptionCode = exceptionCode;
        }

        /// <summary>
        /// 异常代码
        /// </summary>
        public byte ExceptionCode { get; private set; }
    }
}
