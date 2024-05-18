using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WestLakeShape.Common;
using WestLakeShape.Common.LogService;
using WestLakeShape.Common.WpfCommon;
using WestLakeShape.Motion.Device;

namespace NanoImprinter.Model
{
    public interface IDeviceManager
    {
        Dictionary<string, IPlatform> Platforms { get; }      
        /// <summary>
        /// 记录当前流程执行到哪里，并保存，下次继续执行
        /// </summary>
        int ProcedureIndex { get; set; }
        bool IsConnected { get; set; }
        IOManager IoManager { get;}
        AxisManager AxesManager { get; }
        string ConfigFileName { get; set;}
        MachineModelConfig Config { get; }
        void LoadParam();
        void SaveParam();
        IPlatform GetPlatform(string name);
        void ConnectedPlatform();
    }

    public class DeviceManager : NotifyPropertyChanged, IDeviceManager
    {
        //private readonly string Config_File_Name = "MachineConfig.config";
        //private readonly string _rootFolder = @"D:\NanoImprinterConfig\";
        private bool _isConnected;
        private string _configFileName;
        private static readonly ILogger _log = LogHelper.For<TrioAxis>();
        private const string _hardwareFile = "Hardware.config";//电路相关的基本参数文件

        /// <summary>
        /// 所有IO卡
        /// </summary>
        public IOManager IoManager { get; private set; }
        /// <summary>
        /// 所有轴
        /// </summary>
        public AxisManager AxesManager { get; private set; }

        /// <summary>
        /// 上次流程执行到的步骤
        /// </summary>
        public int ProcedureIndex { get; set; }

        public MachineModelConfig Config { get; private set; }
        public HardwareConfig HardwareConfig { get; private set; }

        public Dictionary<string, IPlatform> Platforms { get; private set; }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string ConfigFileName
        {
            get => _configFileName;
            set => _configFileName = value;
        }

        public DeviceManager()
        {
            Platforms = new Dictionary<string, IPlatform>();
            //获取config的文件名
            LoadParamFileName();
            //加载参数
            LoadParam();

            RefreshDataService.Instance.Register(RefreshRealtimeData);
        }

        public void SaveParam()
        {
            //var path = _rootFolder + Config_File_Name;
            var path = Constants.ConfigRootFolder +_configFileName ;
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(path, JsonConvert.SerializeObject(Config,Formatting.Indented), Encoding.UTF8);
            SaveHardwareConfig();
        }

        public void LoadParam()
        {
            //var model = new NanoImprinterModel();
            //var configFile = Path.Combine(_rootFolder, Config_File_Name);
            var configFile = Path.Combine(Constants.ConfigRootFolder, _configFileName);
            
            if (File.Exists(configFile))
            {
                //var content = File.ReadAllText(configFile, Constants.Encoding);
                var content = File.ReadAllText(configFile, Encoding.UTF8);
                Config = JsonConvert.DeserializeObject<MachineModelConfig>(content);
            }
            else
            {
                Config = new MachineModelConfig();
                SaveParam();
            }
            //加载硬件参数（不可修改）
            LoadHardwareConfig();
            //对象初始化
            Initial();
        }

        private void Initial()
        {
            //IOStates = new ImprinterIO(Config.ImprinterIO);
            //Axes = new ImprinterAxis(Config.ImprinterAxis);
            IoManager = new IOManager(HardwareConfig.IOManager);
            AxesManager = new AxisManager(HardwareConfig.AxisManager);

            Platforms.Add(typeof(ImprintPlatform).Name, new ImprintPlatform(Config.ImprintPlatform,
                                                                            AxesManager.PrintPlatformAxes()));
            Platforms.Add(typeof(GluePlatform).Name, new GluePlatform(Config.GluePlatform,
                                                                      AxesManager.GluePlatformAxes()));
            Platforms.Add(typeof(AfmPlatform).Name, new AfmPlatform(Config.AfmPlatform,
                                                                    AxesManager.AFMPlatformAxes()));
            Platforms.Add(typeof(MicroPlatform).Name, new MicroPlatform(Config.MicroPlatform));
            Platforms.Add(typeof(MacroPlatform).Name, new MacroPlatform(Config.MacroPlatform,
                                                                        AxesManager.MacroPlatformAxes()));
        }

        /// <summary>
        /// 重要文件，只允许程序修改者修改
        /// </summary>
        public void LoadHardwareConfig()
        {
            try
            {
                var axisFile = Path.Combine(Constants.ConfigRootFolder, _hardwareFile);
                if (File.Exists(axisFile))
                {
                    var content = File.ReadAllText(axisFile, Encoding.UTF8);
                    HardwareConfig = JsonConvert.DeserializeObject<HardwareConfig>(content);
                }
                else
                {
                    //HardwareConfig = new HardwareConfig();
                    //HardwareConfig.ImprinterAxis = Config.ImprinterAxis;
                    //HardwareConfig.ImprinterIO = Config.ImprinterIO;
                    //SaveHardwareConfig();
                    throw new Exception("轴的基本参数文件不存在，请拷贝！同时：慎重修改，否则撞机！");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("轴的基本参数文件报错，慎重修改，否则撞机！" + ex.Message);
            }        
        }

        public void SaveHardwareConfig()
        {
            var path = Constants.ConfigRootFolder + _hardwareFile;
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(path, JsonConvert.SerializeObject(HardwareConfig, Formatting.Indented), Encoding.UTF8);
        }


        public IPlatform GetPlatform(string type)
        {
            if (!Platforms.TryGetValue(type, out var platform))
            {
                return null;
            }
            return platform;
        }

        public void ConnectedPlatform()
        {
            if (!IsConnected)
            {
                AxesManager.Connect();
                IoManager.Connect();
                //foreach (var pairs in Platforms)
                //{
                //    pairs.Value.Connect();
                //}

                IsConnected = true;
            }
            else
            {
                AxesManager.Disconnect();

                //foreach (var pairs in Platforms)
                //{
                //    pairs.Value.Disconnect();
                //}
                IsConnected = false;
            }
        }

        public void LoadParamFileName()
        {
            var configFile = Path.Combine(Constants.ConfigRootFolder, "参数文件名.txt");

            var path = Constants.ConfigRootFolder + _configFileName;
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (File.Exists(configFile))
            {
                _configFileName = File.ReadAllText(configFile);
            }
            else
            {
                _configFileName = "MachineConfig.config";
            }
        }

        public void SaveParamFileName()
        {
            var configFile = Path.Combine(Constants.ConfigRootFolder, "参数文件名.txt");

            if (File.Exists(configFile))
            {
                File.WriteAllText(configFile, _configFileName);
            }
        }

        private void RefreshRealtimeData()
        {
            var isAll = Platforms.Values.All(o => o.IsConnected);
            //IsConnected = isAll;
        }
    }


    public class MachineModelConfig
    {
        public WafeInfo WafeInfo { get; set; } = new WafeInfo();
        public MaskInfo MaskInfo { get; set; } = new MaskInfo();
        //public ImprinterAxisConfig ImprinterAxis { get; set; } = new ImprinterAxisConfig();
        //public ImprinterIOConfig ImprinterIO { get; set; } = new ImprinterIOConfig();

        public AfmPlatformConfig AfmPlatform { get; set; } = new AfmPlatformConfig();
        public GluePlatformConfig GluePlatform { get; set; } = new GluePlatformConfig();
        public MacroPlatformConfig MacroPlatform { get; set; } = new MacroPlatformConfig();
        public MicroPlatformConfig MicroPlatform { get; set; } = new MicroPlatformConfig();
        public ImprintPlatformConfig ImprintPlatform { get; set; } = new ImprintPlatformConfig();
    }


    /// <summary>
    /// 硬件固定参数，不能随意修改。
    /// </summary>
    public class HardwareConfig
    {
        public AxisManagerConfig AxisManager { get; set; } = new AxisManagerConfig();
        public IOManagerConfig IOManager { get; set; } = new IOManagerConfig();
    }

}
