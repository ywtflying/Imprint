using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WestLakeShape.Common.WpfCommon;

namespace WestLakeShape.Motion.Device
{
    /// <summary>
    /// 执行动作的压电陶瓷机构
    /// 闭环可以读取到位移，开环读取的电压
    /// </summary>
    public class PiezoActuator
    {
        private bool[] _isClosedLoop;          //开环读取的是电压，闭环读取的是位移
        private PiezoActuatorConfig _config;
        private PiezoSerialPort _piezoPort;

        private readonly byte Read_Closed_Loop_Flag = 0x43;
        private readonly byte Read_Opened_Loop_Flag = 0x4F;
        private readonly byte Write_Closed_Loop_Flag = 0xC;
        private readonly byte Write_Opened_Loop_Flag =0x0;

        public bool IsConnected => _piezoPort.IsConnected;

        public bool this[int i]
        {
            get => _isClosedLoop[i];
            set => _isClosedLoop[i] = value;
        }

        public bool IsClosedLoop => _isClosedLoop[1];

        public PiezoActuator(PiezoActuatorConfig config)
        {
            _config = config;
            _piezoPort = new PiezoSerialPort(config.PortName);
            _isClosedLoop = new bool[3];
        }

        public void Connect()
        {
            try
            {
                _piezoPort.Connected();
            }
            catch (Exception e)
            {
                throw e;
            }   
        }

        public void Disconnected()
        {
            _piezoPort.Disconnected();
        }

        public void ReloadConfig()
        {
            if (_piezoPort.IsConnected)
                Disconnected();

            _piezoPort.PortName = _config.PortName;

            _piezoPort.Connected();
        }

        public void SetClosedLoop(int channelNo,bool isClosedLoop)
        {
            var value = isClosedLoop ? Read_Closed_Loop_Flag : Read_Opened_Loop_Flag;
            //发送修改ClosedLoop的命令，
            _piezoPort.SendCommand(B3Commands.EnableClosedLoop, new int[] { channelNo, value });
            
            ReadClosedLoopFlag();
        }
        public void SetAllChannelClosedLoop(bool isClosedLoop)
        {
            var value = isClosedLoop ? Write_Closed_Loop_Flag : Write_Opened_Loop_Flag;
            var openChannels = ClassifyChannels(false);
            openChannels.ForEach(index =>
            {
                _piezoPort.SendCommand(B3Commands.EnableClosedLoop, new int[] { index, value });
                
                ReadClosedLoopFlag();
            });
           
        }

        /// <summary>
        /// 写单路位移
        /// </summary>
        /// <param name="no"></param>
        /// <param name="disp"></param>
        public void WriteDisplace(ChannelNo no,double disp)
        {
            _piezoPort.SendData(B3Commands.WriteSingleChannelDisp, (int)no, new double[] { disp });
        }

        /// <summary>
        /// 写多路位移
        /// </summary>
        /// <param name="disp"></param>
        public void WriteMultiDisplace(double[] disp)
        {
            _piezoPort.SendData(B3Commands.WriteMultiChannelsDisp, (int)ChannelNo.One, disp);
        }

        /// <summary>
        /// 读单路位移
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public double ReadDisplace(ChannelNo no)
        {
            _piezoPort.SendCommand(B3Commands.ReadSingleChannelDisp, new int[] { (int)no });
            var temp = _piezoPort.ReceiveValues(B3Commands.ReadSingleChannelDisp, (int)no, 1);
           
            return temp[0];
        }

        /// <summary>
        /// 读多路位移
        /// </summary>
        /// <param name="disp"></param>
        /// <returns></returns>
        public double[] ReadMultiDisplace()
        {
            //_piezoPort.SendCommand(B3Commands.ReadMultiChannelDispOrV);
            //var temp = _piezoPort.ReceiveValues(B3Commands.ReadMultiChannelDispOrV, 0, 3);
            //return temp;

            var positions = new double[3];
            
            foreach (var no in Enum.GetValues(typeof(ChannelNo)))
            {
                var i = (int)no;
                _piezoPort.SendCommand(B3Commands.ReadSingleChannelDisp, new int[] { (int)no });
                var temp = _piezoPort.ReceiveValues(B3Commands.ReadSingleChannelDisp, (int)no, 1);
                positions[(int)no] = temp[0];
            }

            return positions;
        }

        /// <summary>
        /// 标定载荷
        /// </summary>
        /// <param name="loadValue"></param>
        /// <param name="no"></param>
        /// <returns></returns>
        public bool LoadCalib(ChannelNo no,double loadValue)
        {
            _piezoPort.SendData(B3Commands.LoadCalib, (int)no, new double[] { loadValue });
            return true;
        }

        /// <summary>
        /// 设定位移的限位
        /// </summary>
        /// <param name="no"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool SetDisplaceSoftLimit(ChannelNo no,double position)
        {
            _piezoPort.SendData(B3Commands.DisplaceSoftLimit,(int)no,new double[] {position});
            return true;
        }

        /// <summary>
        /// 读取位移的限位
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public double ReadDisplaceSoftLimt(ChannelNo no)
        {
            _piezoPort.SendCommand(B3Commands.DisplaceSoftLimit, new int[] { (int)no });

            //var temp = _piezoPort.ReceiveMessage(B3Commands.DisplaceSoftLimit, (int)no, 1,false);
            //return temp[0];
            return 0;
        }

        /// <summary>
        /// 清零多路
        /// </summary>
        /// <returns></returns>
        public bool ClearMultiChannel()
        {
            _piezoPort.SendCommand(B3Commands.ClearMultiChannels);
            return true;
        }

        /// <summary>
        /// 读取通道的开闭环状态
        /// </summary>
        /// <returns></returns>
        public bool ReadClosedLoopFlag()
        {
            var channels = ((ChannelNo[])Enum.GetValues(typeof(ChannelNo))).ToList();
            channels.ForEach(index =>
            {
                //发送数据
                _piezoPort.SendCommand(B3Commands.ReadClosedLoopFlag, new int[] { (int)index });
               
                //读取数据
                var flag = _piezoPort.ReceiveFlag(B3Commands.ReadClosedLoopFlag, (int)index, 1);
                //数据转化
                _isClosedLoop[(int)index] = (flag & Read_Closed_Loop_Flag) == Read_Closed_Loop_Flag ? true : false;
            });

            return true;
        }

        /// <summary>
        /// 根据开闭环进行通道的分类
        /// </summary>
        /// <param name="isClosedLoop"></param>
        /// <returns></returns>
        private List<int> ClassifyChannels(bool isClosedLoop)
        {
            return _isClosedLoop.Select((value, index) => new { Value = value, Index = index })
                                .Where(item => item.Value == isClosedLoop)
                                .Select(item => item.Index)
                                .ToList();
        }
    }


    public class PiezoActuatorConfig : NotifyPropertyChanged
    {
        private string _name = "COM1";
        private int _zMaxValue;
        private int _zMinValue;
        private int _rxMaxValue;
        private int _rxMinValue;
        private int _ryMaxValue;
        private int _ryMinValue;
        public string PortName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public int ZMaxValue
        {
            get => _zMaxValue;
            set => SetProperty(ref _zMaxValue, value);
        }

        public int ZMinValue
        {
            get => _zMinValue;
            set => SetProperty(ref _zMinValue, value);
        }
        public int RXMaxValue
        {
            get => _rxMaxValue;
            set => SetProperty(ref _rxMaxValue, value);
        }
        public int RXMinValue
        {
            get => _rxMinValue;
            set => SetProperty(ref _rxMinValue, value);
        }
        public int RYMaxValue
        {
            get => _ryMaxValue;
            set => SetProperty(ref _ryMaxValue, value);
        }
        public int RYMinValue
        {
            get => _ryMinValue;
            set => SetProperty(ref _ryMinValue, value);
        }
    }


    public enum ChannelNo : int
    {
        One = 0,
        Two = 1,
        Third = 2,
    }


    public enum B3Commands
    {
        //写
        WriteSingleChannelV = 0,
        WriteSingleChannelDisp = 1,
        WriteMultiChannelsV = 2,
        WriteMultiChannelsDisp = 3,
        ClearMultiChannels = 4,

        //读
        ReadSingleChannelV = 5,
        ReadSingleChannelDisp = 6,

        ReadSingleChannelRealtimeV = 7,
        ReadSingleChannelRealtimeDisp = 8,
        ReadMultiChannelsRealtimeV = 9,
        ReadMultiChannelsRealtimeDisp = 10,
        StopReadRealtime = 11,

        EnableClosedLoop = 18,
        ReadClosedLoopFlag = 19,

        LoadCalib =24,

        DisplaceSoftLimit =26,
        //自检类
        StartSelfCheck = 39,
        ReadSelfCheckCode = 40,

        ReadMultiChannelsRealtimeDataH = 48,
        ReadMultiChannelRealtimeDataD = 49,

        ReadMultiChannelDispOrV=51,

    }
}
