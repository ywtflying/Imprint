using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WestLakeShape.Motion
{
    /// <summary>
    /// 通信协议与原始说明书不同，代码重复待后期优化删除
    /// </summary>
    public class UVControlPort
    {
        private SerialPort _port;
        private bool _isConnected;
        private Stream _stream;
        private static int Head_Length = 1;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public bool IsConnected => _isConnected;
        public string PortName
        {
            get => _port.PortName;
            set => _port.PortName = value;
        }

        public UVControlPort(string name)
        {
            _port = new SerialPort()
            {
                PortName = name,
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        public void Connected()
        {
            if (!_isConnected)
            {
                _port.Open();
                _isConnected = true;
                _stream = _port.BaseStream;
            }
        }
         
        public void Disconnected()
        {
            if (_isConnected)
                _port.Close();

            _isConnected = false;
        }


        public bool WriteSingleHoldingRegister(byte slaveId, int address, int value)
        {
            byte lowByte = (byte)(value & 0xFF); // 低位字节
            byte highByte = (byte)((value >> 8) & 0xFF); // 高位字节
            var bytes = new byte[2] { highByte,lowByte };

            var request = CreatePdu(slaveId, FunctionCodes.WriteSingleHoldingRegister, address, bytes);
            SendData(request, 1);

            return true;
        }

        public byte[] ReadHoldingRegisters(byte slaveId, int address, int registerNum)
        {
            byte lowByte = (byte)(registerNum & 0xFF); // 低位字节
            byte highByte = (byte)((registerNum >> 8) & 0xFF); // 高位字节
            var bytes = new byte[2] { highByte, lowByte };

            var request = CreatePdu(slaveId, FunctionCodes.ReadHoldingRegisters, address, bytes);
            _stream.Write(request, 0, request.Length);
            var recvDataLength = 2 * registerNum + 5;

            return DataReceived(recvDataLength);
        }

        public bool WriteMultiRegister(byte slaveId, int address, int registerNum,byte[] values)
        {
            var request = CreatePdu(slaveId, FunctionCodes.WriteRegisters, address, values);
            var data = SendData(request, 1);

            return true;
        }

        private byte[] SendData(byte[] request, int registerNum)
        {
            _semaphore.Wait();
            try
            {
                _stream.Write(request, 0, request.Length);

                var recvDataLength = 2 * registerNum + 6;

                return DataReceived(recvDataLength);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally 
            {
                _semaphore.Release();
            }
        }

        private byte[] CreatePdu(byte slaveId, byte command, int address,byte[] values)
        {
            var buff = new byte[6 + values.Length];
            buff[0] = slaveId;                         //从站地址
            buff[1] = command;                         //命令
            buff[2] = (byte)((address >> 8) & 0xff);   //寄存器地址高位
            buff[3] = (byte)(address & 0xff);          //寄存器地址低位
            Array.Copy(values,0, buff,4, values.Length);
            UpdateCrc16(buff);

            return buff;
        }

        private byte[] DataReceived(int receiveDataSize)
        {
            var buff = new byte[256];
            var right = 0;

            try
            {
                _port.DiscardInBuffer();
                while (right < receiveDataSize)
                {
                    var count = buff.Length - right;
                    if (count <= 0)
                        throw new BufferFullException();

                    var size = _stream.Read(buff, right, count);

                    right += size;
                    //读取不到完整的回复消息，则break
                    if (size == 0 && right != receiveDataSize)
                        break;
                }
                return TakeMessage(buff);
            }
            catch (TimeoutException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private byte[] TakeMessage(byte[] buffer)
        {
            var functionCode = buffer[Head_Length];
            int index = 0;//数据起始地址
            int count = 2;//数据长度
            if (functionCode < 0x80)
            {
                // No exception
                switch (functionCode)
                {
                    case FunctionCodes.ReadHoldingRegisters:
                        index = 3;                //获取寄存器索引
                        count = buffer[index - 1];//获取寄存器数量
                        break;
                    case FunctionCodes.WriteSingleHoldingRegister:
                        index = 4;  
                        count = 2;
                        break;
                    case FunctionCodes.WriteRegisters:
                        index = 4;  
                        count = 2;
                        break;
                    default:
                        throw new UnexpectedValueException(functionCode);
                }
            }
            else
            {
                throw new UnexpectedValueException(functionCode);
            }
            //检查CRC校验
            if (!ValidateCrc16(buffer))
                throw new CrcValidateException();

            var values = new byte[count];
            Array.Copy(buffer, index, values, 0, count);
            return values;
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
        private bool ValidateCrc16(byte[] message)
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
}
