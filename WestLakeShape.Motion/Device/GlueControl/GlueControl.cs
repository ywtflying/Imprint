using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
            ChangeWorkModel(true);
        }

        public void Disconnected()
        {
            _port.Disconnected();
        }
       
        public void ReloadConfig()
        {
            if (_port.IsConnected)
            {
                Disconnected();
                _port.Name = _config.PortName;
                Connect();
            }
            else
            {
                _port.Name = _config.PortName;
            }
        }

        /// <summary>
        /// 开始点胶
        /// </summary>
        /// <returns></returns>
        public bool StartDispense()
        {
            ReadParam(RegisterNo.StartDispense, CommandValue.Start_Dispense);
            WriteParam(RegisterNo.StartDispense, 1);
            return true;
        }

        /// <summary>
        /// 停止点胶
        /// </summary>
        /// <returns></returns>
        public bool StopDispense()
        {
            ReadParam(RegisterNo.StartDispense, CommandValue.Stop_Dispense);
            WriteParam(RegisterNo.StartDispense, 0);
            return true;
        }

        /// <summary>
        /// 保存参数指令
        /// </summary>
        /// <returns></returns>
        public bool SaveParam()
        {
            //设置点胶数
            WriteParam(RegisterNo.DotPoints, _config.GluePoints);          
            // 设置开阀时间
            WriteParam(RegisterNo.OpenTime, _config.OpenTime);
            // 设置关阀时间
            WriteParam(RegisterNo.ClosedTime, _config.ClosedTime);
            // 开阀力度
            WriteParam(RegisterNo.OpenIntensity, _config.OpenIntensity);
            // 关阀力度
            WriteParam(RegisterNo.ClosedIntensity, _config.ClosedIntensity);
            //设置加热温度
            WriteParam(RegisterNo.TargetTemperator, _config.TargetTemperatore);
            //点胶延时
            WriteParam(RegisterNo.DispensingDelayTime, _config.DispensingDelayTime);
           
            //保存参数
            WriteParam(RegisterNo.SaveParamter, CommandValue.Save_Param);
            return true;
        }


        /// <summary>
        /// 清空点胶数
        /// </summary>
        public void ClearPoints()
        {
            WriteParam(RegisterNo.HighGlueCount, 0);
            Thread.Sleep(30);
            WriteParam(RegisterNo.LowerGlueCount, 0);
        }

        /// <summary>
        /// 开始加热
        /// </summary>
        /// <returns></returns>
        public bool StartHeartAction()
        {
            WriteParam(RegisterNo.HeartAction, 1);
            return true;
        }
      
        /// <summary>
        /// 关闭加热
        /// </summary>
        /// <returns></returns>
        public bool StopHeartAction()
        {
            WriteParam(RegisterNo.HeartAction, 0);
            return true;
        }
       
        public int RefreshCurrentTemperator()
        {
            ReadParam(RegisterNo.CurrentTemperator, 1);
            return 1;
        }

        /// <summary>
        /// 设置点胶模式
        /// </summary>
        /// <param name="isLine"></param>
        private void ChangeWorkModel(bool isLine)
        {
            int val = isLine ? 1 : 0;         
            WriteParam(RegisterNo.WorkModel, val);
        }

        private byte[] ReadParam(RegisterNo registerNo, ushort command)
        {
            //return _port.ReadSingleRegister(8194);
            return  _port.ReadSingleRegister((ushort)registerNo);
        }
       
        private bool WriteParam(RegisterNo registerNo, int val)
        {
            //_port.WriteSingleRegister(8713, 1);
            var ss = (ushort)registerNo;
            _port.WriteSingleRegister((ushort)registerNo, (ushort)val);
            return true;
        }

        ///// <summary>
        ///// 写入单个参数
        ///// </summary>
        ///// <param name="registerName"></param>
        ///// <param name="registerValue"></param>
        ///// <returns></returns>
        //public bool DownloadParameter(string registerName, int registerValue)
        //{
        //    if (Enum.IsDefined(typeof(RegisterNo), registerName))
        //    {
        //        var registerNo = (RegisterNo)Enum.Parse(typeof(RegisterNo), registerName, true);
        //        WriteParam(registerNo, registerValue);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 写入所有参数
        ///// </summary>
        ///// <returns></returns>
        //public bool DownloadAllParameter()
        //{
        //    Type configType = _config.GetType();
        //    PropertyInfo[] properties = configType.GetProperties();
        //    foreach (var property in properties)
        //    {
        //        var propertyName = property.Name;
        //        if (Enum.IsDefined(typeof(RegisterNo), propertyName))
        //        {
        //            var registerNo = (RegisterNo)Enum.Parse(typeof(RegisterNo), propertyName, true);
        //            var registerValue = (int)property.GetValue(_config);
        //            WriteParam(registerNo, registerValue);
        //        }
        //    }
        //    return true;
        //}


        enum RegisterNo : ushort
        {
            SaveParamter = 0x2008,
            //ModbusAddress,
            //ModbusBaudrate,
            StartDispense = 0x200B,   //点胶触发
            OpenIntensity = 0x2100,   //重复功能寄存器，找供应商
            Cycle = 0x2101,           //点胶频率
            HighGlueCount = 0x2102,   //点胶计数高
            LowerGlueCount = 0x2103,  //点胶计数低

            //DownTime = 0x2200,               //下降时间
            OpenTime = 0x2201,              //开阀时间
            //UpTime,                          //上升时间
            ClosedTime = 0x2203,            //关阀时间   
            ClosedIntensity = 0x2204,       //开阀力度
            WorkModel = 0x2205,             //点胶模式
            DotPoints = 0x2206,             //点胶数设定
            DispensingDelayTime = 0x2207,   //点胶延时 
            TargetTemperator = 0x2208,      //设定温度
            HeartAction = 0x2209,           //喷嘴加热
            CurrentTemperator = 0x220A,     //实时温度
        }

        static class CommandValue
        {
            public static ushort Start_Dispense = 1;
            public static ushort Stop_Dispense = 0;
            public static ushort Save_Param = 1;
            public static ushort Line_Model = 0;
            public static ushort Dot_Model = 1;
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
        private int _openTime;
        private int _closedTime;
        private int _upTime;
        private int _openIntensity;
        private int _closedIntensity;
        private int _controlModel;
        private int _gluePoints;
        private int _glueCycle;
        private int _dispensingDelayTime;
        private int _targetTemperatore;

        [Category("GlueControl"), Description("Comm端口号")]
        [DisplayName("PortName")]
        public string PortName 
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
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
        public int OpenTime
        {
            get => _openTime;
            set => SetProperty(ref _openTime, value);
        }
        
        [Category("GlueControl"), Description("关阀时间，单位10us"), DefaultValue(30)]
        [DisplayName("ClosedValveTime")]
        public int ClosedTime
        {
            get => _closedTime;
            set => SetProperty(ref _closedTime, value);
        }


        [Category("GlueControl"), Description("上升时间，单位10us"), DefaultValue(20)]
        [DisplayName("UpTime")]
        public int UpTime
        {
            get => _upTime;
            set => SetProperty(ref _upTime, value);
        }

        [Category("GlueControl"), Description("开阀力度，单位%"), DefaultValue(10)]
        [DisplayName("OpenValveIntensity")]
        public int OpenIntensity
        {
            get => _openIntensity;
            set => SetProperty(ref _openIntensity, value);
        }
        [Category("GlueControl"), Description("开阀力度，单位%"), DefaultValue(10)]
        [DisplayName("CloseValveIntensity")]
        public int ClosedIntensity
        {
            get => _closedIntensity;
            set => SetProperty(ref _closedIntensity, value);
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
        [Category("GlueControl"), Description("点胶频率"), DefaultValue(1)]
        [DisplayName("GluePoints")]
        public int GlueCycle
        {
            get => _glueCycle;
            set => SetProperty(ref _glueCycle, value);
        }

        [Category("GlueControl"), Description("点胶延时，单位ms"), DefaultValue(1)]
        [DisplayName("DispensingDelayTime")]
        public int DispensingDelayTime 
        {
            get => _dispensingDelayTime;
            set => SetProperty(ref _dispensingDelayTime, value);
        }

        [Category("GlueControl"), Description("加热温度"), DefaultValue(25)]
        [DisplayName("TargetTemperatore")]
        public int TargetTemperatore
        {
            get => _targetTemperatore;
            set => SetProperty(ref _targetTemperatore, value);
        }
    }
}
