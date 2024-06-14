
using Serilog;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using WestLakeShape.Common.LogService;
using WestLakeShape.Common.WpfCommon;
using static WestLakeShape.Motion.IOStateSource;

namespace WestLakeShape.Motion.Device
{
    public class UVControl
    {
        private static readonly ILogger _log = LogHelper.For<TrioAxis>();
        private UVControlConfig _config;
        //private IOState _io;
        private UVControlPort _port;
        private readonly byte Slave_ID = 1;
        private bool _isAutoFlag = true;    //通信模式为自动模式
        private bool _isOpenFlag = false;   //UVlight是否打开
        private bool _lightIsConnected = false;  //UV通道1连接light
        private bool _isConnected = false;  //UV控制器连接
        private bool _isAlarmFlag = false;  //是否报警
       
       

        public int FirstChannel => (int)UVLightChannel.First;
        public bool IsWorkFlag => _isOpenFlag;
        public bool IsAlarm => _isAlarmFlag;
        public bool IsConnected => _isConnected;

        public bool LightIsConnected => _lightIsConnected;
        
        public UVControl(UVControlConfig config)
        {
            _config = config;
            _port = new UVControlPort(config.PortName);
        }

        public void OnConnecting()
        {
            _port.Connected();
            _isConnected = _port.IsConnected;

            SwitchWorkModel();
            _log.Information("UV连接成功");
        }
        public void OnDisconnecting()
        {
            _port.Disconnected();
            _log.Information("UV断开连接");
        }
        public void ReloadConfig()
        {
            try
            {
                if (_isConnected)
                {
                    OnDisconnecting();
                    _port.PortName = _config.PortName;
                    OnConnecting();
                    _log.Information($"UV串口更改成{_config.PortName}");
                }
                else 
                {
                    _port.PortName = _config.PortName;
                }
            }
            catch (Exception e)
            {
                var msg = $"UV控制器重载参数失败，原因：{e.Message}";
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// 切换工作模式;
        /// 通信模式为自动模式
        /// </summary>
        private void SwitchWorkModel()
        {
            var flag = _isAutoFlag ? 0x0000 : 0x0001;
            _port.WriteSingleHoldingRegister(Slave_ID, 0x9DDD, flag);
        }

        public void Open(int channelNo)
        {
            _port.WriteSingleHoldingRegister(Slave_ID, 0x9C41, channelNo);
        }
        public void Closed(int channelNo)
        {
            _port.WriteSingleHoldingRegister(Slave_ID, 0x9C41, channelNo * 2);
        }


        public void OpenAllChannel()
        {
            _port.WriteSingleHoldingRegister(Slave_ID, 0x9C41, (int)UVLightChannel.All);
        }
        public void CloseAllChannel()
        {
            _port.WriteSingleHoldingRegister(Slave_ID, 0x9C41, (int)UVLightChannel.All * 2);
        }

        /// <summary>
        /// 打开蜂鸣器
        /// </summary>
        public void OpenBuzzer()
        {
            _port.WriteSingleHoldingRegister(Slave_ID, 0x9DDC, 0x0001);
        }

        /// <summary>
        /// 关闭蜂鸣器
        /// </summary>
        public void CloseBuzzer()
        {
            _port.WriteSingleHoldingRegister(Slave_ID, 0x9DDC, 0x0000);
        }

        /// <summary>
        /// 写照射参数
        /// </summary>
        public void WriteIrradiationParameter()
        {
            var time = _config.IrradiationTime * 10;

            byte timeLow = (byte)(time & 0xFF); // 低位字节
            byte timeHigh = (byte)((time >> 8) & 0xFF); // 高位字节
            
            byte powerLow = (byte)(_config.PowerPercentage & 0xFF); // 低位字节
            byte powerHigh = (byte)((_config.PowerPercentage >> 8) & 0xFF); // 高位字节

            var values = new byte[39] {
            0x00,0x12,0x24,
            0x00,0x00,0x00,0x01,//1段照射恒功率
            timeHigh,timeLow,
            0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,
            powerHigh,powerLow,
            0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00
            };

            _port.WriteMultiRegister(Slave_ID, 0x9C48, 18, values);
            ReadIrradiationParameter();
        }

        public void ReadIrradiationParameter()
        {
            var bytes = _port.ReadHoldingRegisters(Slave_ID, 0x9C48, 18);
            //01 03 报头 24寄存器数量
            //00 01 采用多通道
            //00 05 照射分5段
         
            var datas = new int[bytes.Length / 2];

            for (int i = 0; i < datas.Length; i++)
            {
                datas[i] = BitConverter.ToInt16(new byte[2] { bytes[2*i+1], bytes[2 * i]},0);
            }
            if (datas[0] != 0)
            {
                _log.Information("UV设定为多阶段不同功率照射");
                throw new Exception("UV为多阶段不同功率照射");
            }
            if (datas[1] != 1)
            {
                _log.Information("UV设定为多阶段不同功率照射");
                throw new Exception("UV为多阶段不同功率照射");
            }
            var time = datas[2] / 10;
            if (_config.IrradiationTime != time)
                throw new Exception("控制器功率参数与保存参数不一致");
            if(_config.PowerPercentage != datas[10])
                throw new Exception("控制器照射参数与保存参数不一致");
        }

        public void ReadWorkStatus()
        {
            var bytes = _port.ReadHoldingRegisters(Slave_ID, 0x9CA5, 1);
            var value = Convert.ToInt16(bytes);
            var status = (UVControlStatus)value;

            if (IsBitSet(value, UVControlStatus.FirstChannelConnect))
            {
                _isConnected = false;
                _log.Information("UV灯未连接");
                throw new Exception("UV通道1未连接镜头,请检查！");
            }
            if (status.HasFlag(UVControlStatus.FirstChannelOpen))
            {
                _isOpenFlag = true;
                _log.Information("UV已打开");
            }

            if (status.HasFlag(UVControlStatus.FirstChannelClose))
            {
                _isOpenFlag = false;
                _log.Information("UV已关闭");
            }

            if (status.HasFlag(UVControlStatus.FirstChannelLEDAlarm) ||
                status.HasFlag(UVControlStatus.FirstChannelTemperatureAlarm))
            {
                _isAlarmFlag = true;
                _isOpenFlag = false;
                _log.Information($"UV报警，状态值：{status}");
            }
            Thread.Sleep(5);
        }

        private static bool IsBitSet(short value, UVControlStatus bit)
        {
            return (value & (short)bit) == 0;
        }

        /// <summary>
        /// UV灯源通道
        /// </summary>
        enum UVLightChannel
        {
            First = 0x0100,
            Second = 0x1000,
            Third = 0x0001,
            Four = 0x0010,
            All = 0x1111,
        }

        [Flags]
        enum UVControlStatus : ushort
        {
            Bit0 = 0x0001,
            Bit1 = 0x0002,
            Bit2 = 0x0004,
            Bit3 = 0x0008,
            Bit4 = 0x0010,
            Bit5 = 0x0020,
            Bit6 = 0x0040,
            Bit7 = 0x0080,
            Bit8 = 0x0100,
            Bit9 = 0x0200,
            Bit10 = 0x0400,
            Bit11 = 0x0800,
            Bit12 = 0x1000,
            Bit13 = 0x2000,
            Bit14 = 0x4000,
            Bit15 = 0x8000,

            //UV控制器的flag
            FirstChannelConnect = Bit0,
            FirstChannelOpen = Bit0,
            FirstChannelClose = Bit1,
            FirstChannelLEDAlarm = Bit0|Bit1,
            FirstChannelTemperatureAlarm = Bit2,

            SecondChannelConnect = Bit4,
            SecondChannelOpen = Bit4,
            SecondChannelClose = Bit5,
            SecondChannelLEDAlarm = Bit4 | Bit5,
            SecondChannelTemperatureAlarm = Bit6,

            ThirdChannelConnect = Bit8,
            ThirdChannelOpen = Bit8,
            ThirdChannelClose = Bit9,
            ThirdChannelLEDAlarm = Bit8 | Bit9,
            ThirdChannelTemperatureAlarm = Bit10,

            FourChannelConnect = Bit12,
            FourChannelOpen = Bit12,
            FourChannelClose = Bit13,
            FourChannelLEDAlarm = Bit12 | Bit13,
            FourChannelTemperatureAlarm = Bit14,
        }
    }


    public class UVControlConfig:NotifyPropertyChanged
    {
        private string _portName = "Comm1";
        private int _irradiationTime;
        private int _powerPercentage;

        [Category("UVControl"), Description("UV com串口")]
        [DisplayName("Com端口")]
        public string PortName 
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        [Category("UVControl"), Description("UV灯保压时间，单位为毫秒")]
        [DisplayName("保压时间")]
        public int IrradiationTime
        {
            get => _irradiationTime;
            set => SetProperty(ref _irradiationTime, value);
        }

        [Category("UVControl"), Description("UV曝光时间.,单位为毫秒")]
        [DisplayName("UV曝光时间")]
        public int PowerPercentage
        {
            get => _powerPercentage;
            set => SetProperty(ref _powerPercentage, value);
        }
    }
}
