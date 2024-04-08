using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WestLakeShape.Motion.Device
{
    /// <summary>
    /// 对供应商的类做了新的改进
    /// 测试后改成异步
    /// </summary>
    public class PiezoSerialPort
    {
        private SerialPort _port;
        private bool _isConnected;
        private Stream _stream;

        private readonly byte Start_Bytes = 0xAA;
        private readonly byte Mcu_Address = 1;
        private readonly byte B4_Alt_Command = 0;
        private readonly int Pdu_Offset = 5;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        public bool IsConnected => _port.IsOpen;

        public string PortName
        {
            get => _port.PortName;
            set => _port.PortName = value;
        }
        public PiezoSerialPort(string name)
        {
            _port = new SerialPort()
            {
                PortName = name,
                BaudRate = 9600,
                DataBits = 8,
                StopBits =StopBits.One,
                Parity = Parity.None,
                ReadTimeout=5000,
                WriteTimeout =5000
            };
        }


        public void Connected()
        {
            try
            {
                if (!_isConnected)
                    _port.Open();
                _isConnected = true;
                _stream = _port.BaseStream;

            }
            catch (Exception e)
            {
                throw new Exception($"微动平台连接失败，{e.Message}");
            }
        }
        

        public void Disconnected()
        {
            if (_isConnected)
                _port.Close();

            _isConnected = false;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="command"></param>
        /// <param name="channelNo"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool SendData(B3Commands command, int channelNo, double[] values)
        {
            _semaphore.Wait();
            try
            {
                var buff = CreatePdu(command, channelNo, values);

                _stream.Write(buff, 0, buff.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _semaphore.Release();
            }
            
            return true;
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool SendCommand(B3Commands command,int[] values= null)
        {
           
            int size = 0;
            if (values != null)
                size = values.Length;
            var buff = new byte[5 + size + 1];
            _semaphore.Wait();
            try
            {
                buff[0] = Start_Bytes;          //起始字节
                buff[1] = Mcu_Address;          //地址
                buff[2] = (byte)buff.Length;    //包长
                buff[3] = (byte)command;        //B3命令
                buff[4] = (byte)B4_Alt_Command; //B4命令

                if (values != null)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        buff[5 + i] = (byte)values[i];
                    }
                }
                buff[buff.Length - 1] = ParityCheck(buff);

                _stream.Write(buff, 0, buff.Length);
               
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _semaphore.Release();
            }

            return true;
        }


        /// <summary>
        /// 接受数据
        /// </summary>
        /// <param name="command"></param>
        /// <param name="channelNo"></param>
        /// <param name="channelNum"></param>
        /// <returns></returns>
        public double[] ReceiveValues(B3Commands command, int channelNo,int channelNum)
        {
            _semaphore.Wait();
            try
            {
                //后期是否考虑对比command，校验位，保证信息的完整性
                //可通过消息的第三个字节获取到消息长度。
                _port.DiscardInBuffer();
                //计算接受数据长度
                var  length= channelNum * 4 + ContainFlagBit(command);
                var startIndex = Pdu_Offset + ContainFlagBit(command);
                var buff = new byte[5 + length + 1];
                var right = 0;//接收到数据的个数
               
                while (right < buff.Length)
                {
                    var count = buff.Length - right;
                    if (count <= 0)
                        throw new BufferFullException();

                    var size = _stream.Read(buff, right, count);

                    right += size;
                    //读取不到完整的回复消息，则break
                    if (size == 0 && right != 8)
                        break;
                }

                ///拷贝数据到数组
                var bytes = new byte[channelNum * 4];
                Array.Copy(buff, startIndex, bytes, 0, channelNum * 4);

                return ConvertToDouble(bytes).ToArray();
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

        public byte ReceiveFlag(B3Commands command, int channelNo, int channelNum)
        {
            var right = 0;
            var buff = new byte[8];
            _semaphore.Wait();
            try
            {
                while (right < 8)
                {
                    var count = buff.Length - right;
                    if (count <= 0)
                        throw new BufferFullException();

                    var size = _stream.Read(buff, right, count);

                    right += size;
                    //读取不到完整的回复消息，则break
                    if (size == 0 && right != 8)
                        break;

                    //后期是否考虑对比command，校验位，保证信息的完整性
                    //可通过消息的第三个字节获取到消息长度。
                    Debug.WriteLine($"通道{channelNo}");
                    DebugInfo(buff);
                }
                return buff[Pdu_Offset];
            }
            catch (TimeoutException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        private void DebugInfo(byte[] data)
        {
            foreach (var b in data)
            {
                Debug.WriteLine($"数据为：{b.ToString("X2")}");
            }
          
        }

        private int ContainFlagBit(B3Commands command)
        {
            return (command == B3Commands.ReadMultiChannelDispOrV || 
                    command == B3Commands.ReadMultiChannelRealtimeDataD ||
                    command == B3Commands.ReadSingleChannelDisp) ? 
                    1 : 0;
        }

        private int ContainChannelBit(B3Commands command)
        {
            return (command == B3Commands.WriteSingleChannelV ||
                    command == B3Commands.WriteSingleChannelDisp) ?
                   1 : 0;
        }

       /// <summary>
       /// 创建PDU
       /// </summary>
       /// <param name="command"></param>
       /// <param name="channelNo"></param>
       /// <param name="values"></param>
       /// <returns></returns>
        private byte[] CreatePdu(B3Commands command, int channelNo, double[] values)
        {
            //一个数据4byte
            var size = 4 * values.Length + ContainChannelBit(command);
            //5byte为header，1byte为校验位
            var buff = new byte[5 + size + 1];
            buff[0] = Start_Bytes;          //起始字节
            buff[1] = Mcu_Address;          //地址
            buff[2] = (byte)buff.Length;    //包长
            buff[3] = (byte)command;        //B3命令
            buff[4] = (byte)B4_Alt_Command; //B4命令
            buff[5] = (byte)channelNo;      //通道号
            var offset = ContainChannelBit(command);
            //转化要发送的数据
            var bytes = ConvertToByte(values);

            for (var i = 0; i < bytes.Length; i++)
                buff[Pdu_Offset + offset + i] = bytes[i];
            
            //校验位
            buff[buff.Length - 1] = ParityCheck(buff);

            return buff;
        }


        /// <summary>
        /// bytes转化成data（double）
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public List<double> ConvertToDouble(byte[] bytes)
        {
            var datas = new List<double>();
            ///4个byte为一通道数据; 不到两个为其他数据
            if (bytes.Length % 4 == 0)
            {
                for (var i = 0; i < bytes.Length / 4; i += 4)
                {
                    double k = (bytes[0] & 0x80) == 0x80 ? -1 : 1;
                    var integer = bytes[0] * 256 + bytes[1];
                    var deci = (bytes[2] * 256 + bytes[3]) * 0.0001;
                    if (integer > 2000)
                    {
                        datas.Add(k * (deci));
                    }
                    else
                    {
                        datas.Add(k * (integer + deci));
                    }
                }
            }
            else
            {
                for (var i = 0; i < bytes.Length; i++)
                    datas.Add((double)bytes[i]);
            }

            Debug.WriteLine($"{datas[0]}");
            return datas;
        }


        /// <summary>
        /// date转化bytes
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public byte[] ConvertToByte(double[] values)
        {
            var bytes = new byte[values.Length * 4];

            for(int i=0;i<values.Length;i++)
            {
                var index = i * 4;
                //计算绝对值
                var absValue = Math.Abs(values[i]);
                //计算小数点
                var decimalValue = absValue - (int)absValue;
                
                if (values[i] < 0)
                {
                    bytes[index] = (byte)(absValue / 256 + 0x80);//将F中的内容转换为负数
                    bytes[index + 1] = (byte)(absValue % 256);
                    decimalValue = (int)(decimalValue * 10000);
                    bytes[index + 2] = (byte)(decimalValue / 256);
                    bytes[index + 3] = (byte)(decimalValue % 256);
                }
                else
                {
                    
                    bytes[index] = (byte)(absValue / 256);
                    bytes[index + 1] = (byte)(absValue % 256);
                    decimalValue = (int)((decimalValue + 0.000001) * 10000);
                    bytes[index + 2] = (byte)(decimalValue / 256);
                    bytes[index + 3] = (byte)(decimalValue % 256);
                }
            }
            return bytes;
        }


        /// <summary>
        /// 计算校验位
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte ParityCheck(byte[] data)
        {
            var parityValue = data[0];
            for (var i = 1; i < data.Length; i++)
            {
                parityValue ^= data[i];
            }
            return parityValue;
        }
    }
}
