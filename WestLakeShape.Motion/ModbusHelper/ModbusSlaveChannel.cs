using System.IO;

namespace WestLakeShape.Motion
{
    public abstract class ModbusSlaveChannel
    {
        /// <summary>
        /// 是否连接
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// PDU之前的字节数（功能码之前）
        /// </summary>
        public abstract ushort HeadLength { get; }


        public abstract void Disconnect();

        public abstract Stream Connect();

        /// <summary>
        /// PDU之后的字节数
        /// </summary>
        public abstract ushort TailLength { get; }

        /// <summary>
        /// 预处理请求消息（写入CRC校验，生成事务ID等），返回事务ID
        /// </summary>
        /// <param name="requestAdu"></param>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        public abstract ushort PrepareRequest(byte[] requestAdu, byte slaveID);

        /// <summary>
        /// 预处理回应消息（校验CRC,读取事务ID等），返回事务ID
        /// </summary>
        /// <param name="responseAdu"></param>
        /// <returns></returns>
        public abstract ushort PrepareResponse(byte[] responseAdu);
    }
}
