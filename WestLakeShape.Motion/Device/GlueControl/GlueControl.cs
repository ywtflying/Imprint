using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WestLakeShape.Common.WpfCommon;

namespace WestLakeShape.Motion.Device
{
    public class GlueControl
    {
        private GlueControlPort _port;
        private const byte Slave_ID = 1;
        private GlueControlConfig _config;

        public bool IsConnected => _port.IsConnected;
        
        /// <summary>
        /// 已点胶次数
        /// </summary>
        public int PointsCount { get; set; }

        /// <summary>
        /// 公式计算
        /// </summary>
        public int GlueCycle { get; set; }

        public GlueControlConfig Config
        {
            get => _config;
            set => _config = value;
        }

        public GlueControl(GlueControlConfig config)
        {
            _config = config;
            _port = new GlueControlPort(config.PortName);
        }

        public void Connect()
        {
            _port.Connected();
            
            //修改成点胶模式
            ChangeWorkModel(false);
        }

        public void Disconnected()
        {
            _port.Disconnected();
        }

        /// <summary>
        /// 开始点胶
        /// </summary>
        /// <returns></returns>
        public bool StartDispense()
        {
            ReadParam(RegisterNo.StartDispense, CommandValue.Start_Dispense);
            WriteParam(RegisterNo.StartDispense, 0);
            return true;
        }

        /// <summary>
        /// 停止点胶
        /// </summary>
        /// <returns></returns>
        public bool StopDispense()
        {
            ReadParam(RegisterNo.StartDispense, CommandValue.Stop_Dispense);
            return true;
        }

        /// <summary>
        /// 保存参数指令
        /// </summary>
        /// <returns></returns>
        public bool SaveParam()
        {
            ReadParam(RegisterNo.SaveParamter, CommandValue.Save_Param);
            return true;
        }

        public void ReloadConfig()
        {
            if (_port.IsConnected)
                Disconnected();

            _port.Name=_config.PortName;

            Connect();
        }

        /// <summary>
        /// 点胶延时
        /// </summary>
        /// <returns></returns>
        public bool WriteDispensingDeleyTime()
        {
            WriteParam(RegisterNo.DispensingDelayTime, _config.DispensingDelayTime);
            return true;
        }

        ///点模式，设置点胶个数
        public bool SetDotCount(int count)
        {
            WriteParam(RegisterNo.DotCount, count);
            return true;
        }
        


        private void ChangeWorkModel(bool isLine)
        {
            int value = isLine ? 1 : 0;         
            WriteParam(RegisterNo.WorkModel, 1);
        }

        private byte[] ReadParam(RegisterNo registerNo, ushort command)
        {
            //return _port.ReadSingleRegister(8194);
            return  _port.ReadSingleRegister((ushort)registerNo);
        }
       
        private bool WriteParam(RegisterNo registerNo, int val)
        {
            //_port.WriteSingleRegister(8713, 1);
            _port.WriteSingleRegister((ushort)registerNo, (ushort)val);
            return true;
        }


        /// <summary>
        /// 写入单个参数
        /// </summary>
        /// <param name="registerName"></param>
        /// <param name="registerValue"></param>
        /// <returns></returns>
        public bool DownloadParameter(string registerName, int registerValue)
        {
            if (Enum.IsDefined(typeof(RegisterNo), registerName))
            {
                var registerNo = (RegisterNo)Enum.Parse(typeof(RegisterNo), registerName, true);
                WriteParam(registerNo, registerValue);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 写入所有参数
        /// </summary>
        /// <returns></returns>
        public bool DownloadAllParameter()
        {
            Type configType = _config.GetType();
            PropertyInfo[] properties = configType.GetProperties();
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                if (Enum.IsDefined(typeof(RegisterNo), propertyName))
                {
                    var registerNo = (RegisterNo)Enum.Parse(typeof(RegisterNo), propertyName, true);
                    var registerValue = (int)property.GetValue(_config);
                    WriteParam(registerNo, registerValue);
                }
            }
            return true;
        }


        enum RegisterNo : ushort
        {
            SaveParamter = 0x2008,
            ModbusAddress,
            ModbusBaudrate,
            StartDispense,
            //OpenValveIntensity=0x2100,重复功能寄存器，找供应商
            Cycle = 0x2101,
            HighGlueCount,
            LowerGlueCount,

            DownTime = 0x2200,
            OpenValveTime,
            UpTime,
            ClosedValveTime,
            OpenValveIntensity,
            WorkModel,
            DotCount,
            DispensingDelayTime,
            TargetTemperatore
        }

        static class CommandValue
        {
            public static ushort Start_Dispense = 1;
            public static ushort Stop_Dispense = 0;
            public static ushort Save_Param = 1;
            public static ushort Line_Model = 0;
            public static ushort Point_Model = 1;
            public static ushort Heating_Enable = 1;
            public static ushort Heating_Disable = 0;
        }
    }

    public class GlueControlConfig:NotifyPropertyChanged
    {
        private string _portName = "Com1";
        private byte _slaveID;
        private ModbusHubConfig _slaveConfig = new ModbusHubConfig();
        private int _downTime;
        private int _openValveTime;
        private int _closedValveTime;
        private int _upTime;
        private int _openValveIntensity;
        private int _controlModel;
        private int _gluePoints;
        private int _dispensingDelayTime;
        private int _targetTemperatore;

        [Category("GlueControl"), Description("Comm端口号")]
        [DisplayName("PortName")]
        public string PortName 
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        } 

        [Category("GlueControl"), Description("从站地址"), DefaultValue(1)]
        [DisplayName("SlaveID")]
        public byte SlaveID 
        {
            get => _slaveID;
            set => SetProperty(ref _slaveID, value);
        }

        [Category("GlueControl"), Description("下降时间，单位10us")]
        [DisplayName("SlaveConfig")]
        public ModbusHubConfig SlaveConfig
        {
            get => _slaveConfig;
            set => SetProperty(ref _slaveConfig, value);
        }


        [Category("GlueControl"), Description("下降时间，单位10us"), DefaultValue(10)]
        [DisplayName("DownTime")]
        public int DownTime
        {
            get => _downTime;
            set => SetProperty(ref _downTime, value);
        }

        [Category("GlueControl"), Description("开阀时间，单位10us"), DefaultValue(30)]
        [DisplayName("OpenValveTime")]
        public int OpenValveTime
        {
            get => _openValveTime;
            set => SetProperty(ref _openValveTime, value);
        }


        [Category("GlueControl"), Description("上升时间，单位10us"), DefaultValue(20)]
        [DisplayName("UpTime")]
        public int UpTime
        {
            get => _upTime;
            set => SetProperty(ref _upTime, value);
        }

        [Category("GlueControl"), Description("关阀时间，单位10us"), DefaultValue(30)]
        [DisplayName("ClosedValveTime")]
        public int ClosedValveTime
        {
            get => _closedValveTime;
            set => SetProperty(ref _closedValveTime, value);
        }

        [Category("GlueControl"), Description("开阀力度，单位%"), DefaultValue(10)]
        [DisplayName("OpenValveIntensity")]
        public int OpenValveIntensity
        {
            get => _openValveIntensity;
            set => SetProperty(ref _openValveIntensity, value);
        }

        [Category("GlueControl"), Description("点胶模式"), DefaultValue(1)]
        [DisplayName("ControlModel")]
        public int ControlModel
        {
            get => _controlModel;
            set => SetProperty(ref _controlModel, value);
        }

        [Category("GlueControl"), Description("点数计算，点模式生效"), DefaultValue(1)]
        [DisplayName("GluePoints")]
        public int GluePoints
        {
            get => _gluePoints;
            set => SetProperty(ref _gluePoints, value);
        }

        [Category("GlueControl"), Description("点胶延时，单位ms"), DefaultValue(1)]
        [DisplayName("DispensingDelayTime")]
        public int DispensingDelayTime 
        {
            get => _dispensingDelayTime;
            set => SetProperty(ref _dispensingDelayTime, value);
        }

        [Category("GlueControl"), Description("温度设定"), DefaultValue(25)]
        [DisplayName("TargetTemperatore")]
        public int TargetTemperatore
        {
            get => _targetTemperatore;
            set => SetProperty(ref _targetTemperatore, value);
        }
    }
}
