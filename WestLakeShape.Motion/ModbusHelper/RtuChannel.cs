using System;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;

namespace WestLakeShape.Motion
{
    public class RtuChannel : ModbusSlaveChannel
    {
        private SerialPort _port;


        public override bool IsConnected => _port.IsOpen;

        public override ushort HeadLength => 1;

        public override ushort TailLength => 2;

        public RtuChannel(PortConfig config)
        {
            _port = new SerialPort
            {
                PortName = config.PortName,
                BaudRate = config.BaudRate,
                StopBits = config.StopBits,
                DataBits = config.DataBit,
                Parity = config.Parity
            };
        }

        public override Stream Connect()
        {
            _port.Open();
            return _port.BaseStream;
        }

        public override void Disconnect()
        {
            _port.Close();
        }

        public override ushort PrepareRequest(byte[] requestAdu, byte slaveId)
        {
            requestAdu[0] = slaveId;

            UpdateCrc16(requestAdu);

            return GenerateTransactionId(requestAdu);
        }

        public override ushort PrepareResponse(byte[] responseAdu)
        {
            if (!ValidateCrc16(responseAdu))
                throw new CrcValidateException();

            return GenerateTransactionId(responseAdu);
        }


        /// <summary>
        /// 根据 Modbus 消息内容，生成消息 ID
        /// </summary>
        /// <param name="message">Modbus消息内容</param>
        private static ushort GenerateTransactionId(byte[] message)
        {
            var slaveId = message[0];
            var functionCode = message[1];
            // 如果大于 0x7f 则为异常响应码，使其最高位变为 0
            if (functionCode > 0x7f)
                functionCode &= 0x7f;
            return (ushort)((slaveId << 8) + functionCode);
        }


        /// <summary>
        /// 计算奇偶校验，并更新到消息内容最后两个字节
        /// </summary>
        /// <param name="request">Modbus消息内容</param>
        private void UpdateCrc16(byte[] request)
        {
            var tmp = ComputeCrc16(request, 0, request.Length - 2);
            request[request.Length - 1] = (byte)(tmp >> 8);
            request[request.Length - 2] = (byte)(tmp & 0xff);
        }


        /// <summary>
        /// 进行 CRC 奇偶校验
        /// </summary>
        public bool ValidateCrc16(byte[] message)
        {
            var tmp = ComputeCrc16(message, 0, message.Length - 2);

            return message[message.Length - 1] == (byte)(tmp >> 8) &&
                message[message.Length - 2] == (byte)(tmp & 0x00ff);
        }

        private static ushort ComputeCrc16(byte[] data, int offset, int count)
        {
            ushort tmp = 0xffff;

            for (var n = 0; n < count; n++)
            {
                tmp = (ushort)((data[offset + n]) ^ tmp);
                for (var i = 0; i < 8; i++)
                {
                    if ((tmp & 0x01) > 0)
                    {
                        tmp = (ushort)(tmp >> 1);
                        tmp = (ushort)(tmp ^ 0xa001);
                    }
                    else
                    {
                        tmp = (ushort)(tmp >> 1);
                    }
                }
            }

            return tmp;
        }
    }

    public class PortConfig
    {
        /// <summary>
        /// 后续更改为.core，为属性添加更多特性
        /// </summary>
        [Description("端口名称，如 Com1"), DefaultValue("Com1")]
        [DisplayName("端口名称")]
        public string PortName { get; set; } = "Com1";

        [Description("波特率，如 115200，98000"), DefaultValue("115200")]
        [DisplayName("波特率")]
        public int BaudRate { get; set; } = 115200;

        [Description("数据位，如 8"), DefaultValue("8")]
        [DisplayName("数据位")]
        public int DataBit { get; set; } = 8;

        [Description("停止位，如 one"), DefaultValue(StopBits.One)]
        [DisplayName("停止位")]
        public StopBits StopBits { get; set; } = StopBits.One;


        [Description("校验位，如 one"), DefaultValue(Parity.None)]
        [DisplayName("校验位")]
        public Parity Parity { get; set; } = Parity.None;

        public static int[] GetBaudRates()
        {
            return new int[]
                {
                    250000,
                    115200,
                    57600,
                    38400,
                    19200,
                    9600,
                    2400,
                    1200
                };
        }

        public static Parity[] GetParity()
        {
            return new Parity[]{
                Parity.None,
                Parity.Odd,
                Parity.Even,
                Parity.Mark,
                Parity.Space
                };
        }
        public override string ToString()
        {
            return $"{PortName}";
        }
    }

    public class CrcValidateException : Exception
    {
        public CrcValidateException() :
            base("CRC 校验失败")
        {
        }
    }
}
