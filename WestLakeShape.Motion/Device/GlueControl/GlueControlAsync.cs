using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace WestLakeShape.Motion.Device
{
    /// <summary>
    /// 当前采用的.framework，prism无法支持异步命令
    /// </summary>
    public class GlueControlTest
    {
        private ModbusSlaveHub _slave;
        private int _currentTemperature;
        private const byte Slave_ID = 1;
        private GlueControlTestConfig _config;

        /// <summary>
        /// 已点胶次数
        /// </summary>
        public int PointsCount { get; set; }

        /// <summary>
        /// 公式计算
        /// </summary>
        public int GlueCycle { get; set; }


        public GlueControlTestConfig Config
        {
            get => _config;
            set => _config = value;
        }


        public GlueControlTest(GlueControlTestConfig config)
        {
            _slave = new ModbusSlaveHub(config.SlaveConfig);

        }

        public bool Connect()
        {
            _slave.OnConnecting();
            return true;
        }

        public bool Disconnected()
        {
            _slave.OnDisconnecting();
            return true;
        }

        /// <summary>
        /// 开始点胶
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartDispense()
        {
            await WriteCommand (RegisterNo.StartDispense, CommandValue.Start_Dispense).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// 停止点胶
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StopDispense()
        {
            await WriteCommand(RegisterNo.StartDispense, CommandValue.Stop_Dispense).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// 保存参数指令
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveParam()
        {
            await WriteCommand(RegisterNo.SaveParamter, CommandValue.Save_Param).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// 点胶延时
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WriteDispensingDeleyTime()
        {
            await WriteParamValue(RegisterNo.DispensingDelayTime, _config.DispensingDelayTime).ConfigureAwait(false);
            return true;
        }


        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="registerNo"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task<bool> WriteCommand(RegisterNo registerNo, ushort command)
        {
            await _slave.WriteSingleRegister(Slave_ID, (ushort)registerNo, command).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// 写入参数
        /// </summary>
        /// <param name="registerNo"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private async Task<bool> WriteParamValue(RegisterNo registerNo, int val)
        {
            await _slave.WriteSingleRegister(Slave_ID, (ushort)registerNo, (ushort)val).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// 写入单个参数
        /// </summary>
        /// <param name="registerName"></param>
        /// <param name="registerValue"></param>
        /// <returns></returns>
        public async Task<bool> DownloadParameter(string registerName, int registerValue)
        {
            if (Enum.IsDefined(typeof(RegisterNo), registerName))
            {
                var registerNo = (RegisterNo)Enum.Parse(typeof(RegisterNo), registerName, true);
                await WriteParamValue(registerNo, registerValue).ConfigureAwait(false);
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
        public async Task<bool> DownloadAllParameter()
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
                    await WriteParamValue(registerNo, registerValue).ConfigureAwait(false);
                }
            }
            return true;
        }


        public async void Refresh()
        {
            await _slave.ReadHoldingRegisters(_config.SlaveID, 0x2102, 2);
            await _slave.ReadHoldingRegisters(_config.SlaveID, 0x2101, 1);
            await _slave.ReadHoldingRegisters(_config.SlaveID, 0x220A, 1);
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
            ControlModel,
            GluePoints,
            DispensingDelayTime,
            TargetTemperatore
        }

        static class CommandValue
        {
            public static ushort Save_Param = 1;

            public static ushort Start_Dispense = 1;
            public static ushort Stop_Dispense = 0;

            public static ushort Line_Model = 0;
            public static ushort Point_Model = 1;
            public static ushort Heating_Enable = 1;
            public static ushort Heating_Disable = 0;
        }
    }


    public class GlueControlTestConfig
    {
        [Category("GlueControl"), Description("从站地址"), DefaultValue(1)]
        [DisplayName("SlaveID")]
        public byte SlaveID { get; set; }

        [Category("GlueControl"), Description("下降时间，单位10us"), DefaultValue(10)]
        [DisplayName("SlaveConfig")]
        public ModbusHubConfig SlaveConfig { get; set; }


        [Category("GlueControl"), Description("下降时间，单位10us"), DefaultValue(10)]
        [DisplayName("DownTime")]
        public int DownTime { get; set; }

        [Category("GlueControl"), Description("开阀时间，单位10us"), DefaultValue(30)]
        [DisplayName("OpenValveTime")]
        public int OpenValveTime { get; set; }


        [Category("GlueControl"), Description("上升时间，单位10us"), DefaultValue(20)]
        [DisplayName("UpTime")]
        public int UpTime { get; set; }

        [Category("GlueControl"), Description("关阀时间，单位10us"), DefaultValue(30)]
        [DisplayName("ClosedValveTime")]
        public int ClosedValveTime { get; set; }

        [Category("GlueControl"), Description("开阀力度，单位%")]
        [DisplayName("OpenValveIntensity")]
        public int OpenValveIntensity { get; set; }

        [Category("GlueControl"), Description("点胶模式")]
        [DisplayName("ControlModel")]
        public int ControlModel { get; set; }

        [Category("GlueControl"), Description("点数计算，点模式生效"), DefaultValue(1)]
        [DisplayName("GluePoints")]
        public int GluePoints { get; set; }

        [Category("GlueControl"), Description("点胶延时，单位ms")]
        [DisplayName("DispensingDelayTime")]
        public int DispensingDelayTime { get; set; }

        [Category("GlueControl"), Description("温度设定"), DefaultValue(25)]
        [DisplayName("TargetTemperatore")]
        public int TargetTemperatore { get; set; }
    }


}
