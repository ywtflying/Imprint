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
    public interface IMachineModel: INotifyPropertyChanged
    {
        Dictionary<string, IPlatform> Platforms { get; }
        
        /// <summary>
        /// 上次流程执行到的步骤
        /// </summary>
        int ProcedureIndex { get; set; }
        bool IsConnected { get; }
        ImprinterIO IOStates { get;}
        ImprinterAxis Axes { get; }
        string ConfigFileName { get; set;}
        MachineModelConfig Config { get; }
        void LoadParam();
        void SaveParam();
        IPlatform GetPlatform(string name);
        void ConnectedPlatform();
        void DisconnectedPlatform();
    }

    public class DeviceModel : IMachineModel
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
        public ImprinterIO IOStates { get; private set; }
        /// <summary>
        /// 所有轴
        /// </summary>
        public ImprinterAxis Axes { get; private set; }

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
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(nameof(_isConnected));
                }
            }
        }

        public string ConfigFileName
        {
            get => _configFileName;
            set => _configFileName = value;
        }

        public DeviceModel()
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

            File.WriteAllText(path, JsonConvert.SerializeObject(Config,Formatting.Indented), Constants.Encoding);
        }

        public void LoadParam()
        {
            //var model = new NanoImprinterModel();
            //var configFile = Path.Combine(_rootFolder, Config_File_Name);
            var configFile = Path.Combine(Constants.ConfigRootFolder, _configFileName);
            
            if (File.Exists(configFile))
            {
                var content = File.ReadAllText(configFile, Constants.Encoding);
                Config = JsonConvert.DeserializeObject<MachineModelConfig>(content);
            }
            else
            {
                Config = new MachineModelConfig();
                SaveParam();
            }
            LoadHardwareConfig();

            IOStates = new ImprinterIO(Config.ImprinterIO);
            Axes = new ImprinterAxis(Config.ImprinterAxis);

            Platforms.Add(typeof(ImprintPlatform).Name, new ImprintPlatform(Config.ImprintPlatform,
                                                                            Axes.PrintPlatformAxes()));
            Platforms.Add(typeof(GluePlatform).Name, new GluePlatform(Config.GluePlatform,
                                                                      Axes.GluePlatformAxes()));
            Platforms.Add(typeof(AfmPlatform).Name, new AfmPlatform(Config.AfmPlatform,
                                                                    Axes.AFMPlatformAxes()));
            Platforms.Add(typeof(MicroPlatform).Name, new MicroPlatform(Config.MicroPlatform));
            Platforms.Add(typeof(MacroPlatform).Name, new MacroPlatform(Config.MacroPlatform,
                                                                        Axes.MacroPlatformAxes()));
            //Instance = model;
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
                    var content = File.ReadAllText(axisFile, Constants.Encoding);
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

            File.WriteAllText(path, JsonConvert.SerializeObject(HardwareConfig, Formatting.Indented), Constants.Encoding);
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
            Axes.Connected();
            //foreach (var pairs in Platforms)
            //{
            //    pairs.Value.Connected();
            //}
            
            _isConnected = true;
        }

        public void DisconnectedPlatform()
        {
            Axes.Disconnected();

            foreach (var pairs in Platforms)
            {
                pairs.Value.Disconnected();
            }

            _isConnected = false;
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
            IsConnected = isAll;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }
    }

    public class MachineModelConfig
    {
        public WafeInfo WafeInfo { get; set; } = new WafeInfo();
        public MaskInfo MaskInfo { get; set; } = new MaskInfo();

        public ImprinterAxisConfig ImprinterAxis { get; set; } = new ImprinterAxisConfig();
        public ImprinterIOConfig ImprinterIO { get; set; } = new ImprinterIOConfig();

        public AfmPlatformConfig AfmPlatform { get; set; } = new AfmPlatformConfig();
        public GluePlatformConfig GluePlatform { get; set; } = new GluePlatformConfig();
        public MacroPlatformConfig MacroPlatform { get; set; } = new MacroPlatformConfig();
        public MicroPlatformConfig MicroPlatform { get; set; } = new MicroPlatformConfig();
        public ImprintPlatformConfig ImprintPlatform { get; set; } = new ImprintPlatformConfig();
      
    }

    /// <summary>
    /// 内部电路文件，不能随意修改。
    /// </summary>
    public class HardwareConfig
    {
        public ImprinterAxisConfig ImprinterAxis { get; set; } = new ImprinterAxisConfig();
        public ImprinterIOConfig ImprinterIO { get; set; } = new ImprinterIOConfig();

    }

}
