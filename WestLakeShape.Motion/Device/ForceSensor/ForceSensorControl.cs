using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WestLakeShape.Common;
using WestLakeShape.Common.WpfCommon;

namespace WestLakeShape.Motion.Device
{
    /// <summary>
    /// 因为各个元器件串口通信标准未定，前期分仪器封装串口，后续优化通信
    /// </summary>
    public class ForceSensorControl
    {
        private readonly byte Slave_ID = 1;
        private readonly int Sensor_Register_Count = 2;
        private readonly int Sensor_Count = 3;
        private SerialPort _port;
        private bool _isConnected;
        private Stream _stream;
        private double[] _forceValues = new double[4];
        private ForceSensorControlConfig _config;
        private bool _isClearZero;
        private readonly byte[] _clearCmd = new byte[11] { 0x01, 0x10, 0x00, 0x5E, 0x00, 0x01, 0x02, 0x00, 0xFF, 0xEB, 0x6E };
        private readonly byte[] _readAllCmd = new byte[8] { 0x01, 0x03, 0x01, 0xC2, 0x00, 0x06, 0x64, 0x6B };

        public double ForceValue0
        {
            get => _forceValues[0];
            private set => _forceValues[0] = value;
        }
        public double ForceValue1
        {
            get => _forceValues[1];
            private set => _forceValues[1] = value;
        }
        public double ForceValue2
        {
            get => _forceValues[2];
            private set => _forceValues[2] = value;
        }
        public double ForceValue3
        {
            get => _forceValues[3];
            private set => _forceValues[3] = value;
        }


        public ForceSensorControl(ForceSensorControlConfig config)
        {
            _config = config;
            _port = new SerialPort()
            {
                PortName = _config.PortName,
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                ReadTimeout = 10000,
                WriteTimeout = 10000
            };
        }
      
        public void Connected()
        {
            try
            {
                if (!_isConnected)
                {
                    _port.Open();
                    _isConnected = true;
                    _stream = _port.BaseStream;
                    ThreadPool.QueueUserWorkItem(_ => TickProc());
                } 
            }
            catch (Exception e)
            {
                throw new Exception($"力传感器报错;{e.Message}");
            }
        }


        public void Disconnected()
        {
            if (_isConnected)
                _port.Close();

            _isConnected = false;
        }

        public void ReloadConfig()
        {
            _port.PortName = _config.PortName;
        }

        public void ClearZero()
        {
            _isClearZero = true;
        }


        private void TickProc()
        {
            while (_isConnected)
            {
                RefreshValues();
            }
        }

        public void RefreshValues()
        {
            var buff = new byte[8];
            CreatRequest(buff);
            try
            {
                //四通道同时清零
                if (_isClearZero)
                {
                    var request = new byte[11] { 0x01, 0x10, 0x00, 0x5E, 0x00, 0x01, 0x02, 0x00, 0xFF, 0xEB, 0x6E };
                    _stream.Write(request, 0, request.Length);
                    Thread.Sleep(15);
                    _isClearZero = false;
                }
                
                //发送数据
                _stream.Write(buff, 0, buff.Length);

                //接受数据
                ReceivedData();
            }
            catch (Exception ex) 
            {
                _isConnected = false;
                throw ex;
            }         
        }

        private void ReceivedData()
        {
            var buffer = new byte[32];
            var right = 0;
            var left = 0;
            //var recDataSize = 3 + 4 * 3 + 2;
            try
            {
                while (right<17)
                {
                    var count = buffer.Length - right;
                    if (count <= 0)
                        throw new BufferFullException();

                    var size = _stream.Read(buffer, right, count);
                   
                    right += size;
                    //读取不到完整的回复消息，则break
                    if (size == 0 && right!=17 )
                        break;
                    Debug.WriteLine($"当前接受数据时，数据起始{right}");
                }
                TryTakeMessage(buffer,right,left);
            }
            catch (TimeoutException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 试图从缓冲区取出一条完整的回复消息
        /// </summary>
        private bool TryTakeMessage(byte[] buffer,int stop,int start)
        {
            var offset = 1;
            var tail = 2;

            var minLength = offset + tail + 1;
            if (buffer.Length < minLength)
                return false;

            var functionCode = buffer[offset];
            int messageLength;
            if (functionCode < 0x80)
            {
                // No exception
                switch (functionCode)
                {
                    case FunctionCodes.ReadHoldingRegisters:
                        {
                            // 0: Function Code, 1: Byte Count, 2+ Data
                            var byteCount = buffer[offset+1];
                            messageLength = offset + 2 + byteCount + tail;
                        }
                        break;
                    case FunctionCodes.ReadInputRegisters:
                        {
                            // 0: Function Code, 1: Byte Count, 2+ Data
                            var byteCount = buffer[offset + 1];
                            messageLength = offset + 2 + byteCount + tail;
                        }
                        break;
                    case FunctionCodes.WriteSingleCoil:
                        // 0: Function Code, 1-2: Register Address, 3-4: Data Value
                        messageLength = offset + 5 + tail;
                        break;
                    case FunctionCodes.WriteRegisters:
                        // 0: Function Code, 1-2: Start Address, 3-4: Data Count
                        messageLength = offset + 5 + tail;
                        break;
                    default:
                        throw new UnexpectedValueException(functionCode);
                }
            }
            else
            {
                throw new UnexpectedValueException(functionCode);
            }

            if (0 < messageLength && messageLength <= stop - start)
            {
                var message = new byte[messageLength];
                
                Array.Copy(buffer, start, message, 0, messageLength);

                if (!ValidateCrc16(message))
                    throw new CrcValidateException();

                ConvertToDouble(message,offset+2,tail);
                return true;
            }
            return false;
        }

        /// <summary>
        /// bytes转化成data（double）
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public void ConvertToDouble(byte[] bytes,int offset,int tail)
        {
            //pdu的头部和尾部
            var dataSize = bytes.Length - offset - tail;
            var count = dataSize / 4;
            ///4个byte为一通道数据
            ///不到两个为其他数据
            if (dataSize % 4 == 0 && count >=3)
            {
                for (var i = 0; i < count; i++)
                {
                    var index = offset + i * 4;
                    bool isPositive = (bytes[index] & 0x80) == 0x80 ? false : true;

                    if (!isPositive)
                    {
                        bytes[index] = (byte)~bytes[index];
                        bytes[index + 1] = (byte)~bytes[index + 1];
                        bytes[index + 2] = (byte)~bytes[index + 2];
                        bytes[index + 3] = (byte)~bytes[index + 3];
                    }

                    var value = BigEndianBitConverter.ToUInt32(bytes,index);

                    if (!isPositive)
                    {
                        value += 1;
                        _forceValues[i] = value;
                        _forceValues[i] = _forceValues[i] *(-1);
                    }
                    else
                    {
                        _forceValues[i] = value;
                    }
                    Debug.WriteLine($"第{i}个传感器读取数值为{ _forceValues[i]}");
                }
            }
            else
            {
                throw new Exception($"当前只有{count}个压力传感器示数");
            }         
        }


        private void CreatRequest(byte[] request)
        {
            request[0] = Slave_ID;
            request[1] = 0x03;
            request[2] = 0x01;
            request[3] = 0xC2;
            request[4] = 0x00;
            request[5] = (byte)(Sensor_Count * Sensor_Register_Count);
            UpdateCrc16(request);
        }

        private void UpdateCrc16(byte[] request)
        {
            var tmp = ComputeCrc16(request, 0, request.Length - 2);
            request[request.Length - 1] = (byte)(tmp >> 8);
            request[request.Length - 2] = (byte)(tmp & 0xff);
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
        
        /// <summary>
        /// 进行 CRC 奇偶校验
        /// </summary>
        private bool ValidateCrc16(byte[] message)
        {
            var tmp = ComputeCrc16(message, 0, message.Length - 2);

            return message[message.Length - 1] == (byte)(tmp >> 8) &&
                message[message.Length - 2] == (byte)(tmp & 0x00ff);
        }


    }


    public class ForceSensorControlConfig : NotifyPropertyChanged
    {
        private string _name;
        public string PortName 
        {
            get => _name?? "com1";
            set => SetProperty(ref _name, value);
        }
    }
}
