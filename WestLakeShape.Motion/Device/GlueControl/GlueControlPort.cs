using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WestLakeShape.Motion.Device
{
    /// <summary>
    /// 当前通信采用自定义协议
    /// </summary>
    public class GlueControlPort
    {
        private readonly ushort Head_Length = 1;
        private readonly ushort Tail_Length = 2;
        private readonly byte Slave_ID = 8;
        private SerialPort _port;
        private Stream _stream;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public bool IsConnected => _port.IsOpen;
        
        public string Name
        {
            get => _port.PortName;
            set => _port.PortName = value;
        }

        public GlueControlPort(string name)
        {
            _port = new SerialPort()
            {
                PortName = name,
                BaudRate = 115200,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }



        public void Connected()
        {
            try
            {
                if (!_port.IsOpen)
                    _port.Open();
                _stream = _port.BaseStream;

            }
            catch (Exception e)
            {
                throw new Exception($"点胶控制器连接失败，{e.Message}");
            }
        }

        public void Disconnected()
        {
            if (_port.IsOpen)
                _port.Close();
        }


        public bool WriteSingleRegister(ushort address, ushort value)
        {
            try
            {
                var request = new byte[11]; ;
                request[0] = Slave_ID;
                request[1] = FunctionCodes.WriteRegisters;
                request[2] = (byte)(address >> 8);//寄存器地址
                request[3] = (byte)(address & 0xff);
                request[4] = 00;//寄存器数量
                request[5] = 01;
                request[6] = 02;//字节数
                request[7] = (byte)(value >> 8);
                request[8] = (byte)(value & 0xff);
                SendData(request, 8);
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public byte[] ReadSingleRegister(ushort address)
        {
            try
            {
                var request = new byte[Head_Length + 5 + Tail_Length]; ;
                request[0] = Slave_ID;
                request[1] = FunctionCodes.ReadHoldingRegisters;
                request[2] = (byte)(address >> 8);
                request[3] = (byte)(address & 0xff);
                request[4] = 00;
                request[5] = 01;

                return SendData(request, 7);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private byte[] SendData(byte[] data, int receiveDataSize)
        {
            _semaphore.Wait();
            try
            {
                UpdateCrc16(data);
                _stream.Write(data, 0, data.Length);
                var ret = DataReceived(receiveDataSize);
                return ret;
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


        private byte[] DataReceived(int receiveDataSize)
        {
            var buff = new byte[16];
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
            int index = 0;
            if (functionCode < 0x80)
            {
                // No exception
                switch (functionCode)
                {
                    case FunctionCodes.ReadHoldingRegisters:
                        index =3;   //获取寄存器数量
                        break;
                    case FunctionCodes.WriteRegisters:
                        index = 2;  //获取寄存器地址
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

            var values = new byte[2];
            Array.Copy(buffer, index, values, 0, 2);
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
