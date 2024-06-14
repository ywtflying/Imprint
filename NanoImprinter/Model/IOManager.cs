using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WestLakeShape.Common.LogService;
using WestLakeShape.Common.WpfCommon;
using WestLakeShape.Motion;
using WestLakeShape.Motion.Device;
using static WestLakeShape.Motion.IOStateSource;

namespace NanoImprinter.Model
{
    public class IOManager
    {
        private IOManagerConfig _config;
        private TrioIOStateSource _ioSource;
        private static readonly ILogger _log = LogHelper.For<IOManager>();
        private TrioControl _trioControl;
        public IOManagerConfig Config => _config;
        public ObservableCollection<StateValue> InputIOs { get; set; }
        public ObservableCollection<StateValue> OutputIOs { get; set; }

        public IOManager(IOManagerConfig config)
        {
            _config = config;
            _trioControl = TrioControl.Instance;

            LoadIOStateSourceConfig();
     
            //根据io配置创建IOState
            _ioSource = new TrioIOStateSource(_config.IOStateSourceConfig);

            Initial();
            RefreshDataService.Instance.Register(RefreshIOState);
        }

        /// <summary>
        /// 完成IO配置
        /// </summary>
        private void LoadIOStateSourceConfig()
        {
            int bitIndex = 23;//轴限位开关占用了13个inputIO，IO接口为15针插头
            var inputIONames = Enum.GetValues(typeof(InputIOName)).Cast<InputIOName>();
            foreach (var io in inputIONames)
            {
                if (!_config.IOStateSourceConfig.States.Any(o => o.Name == io.ToString()))
                {
                    //防止IO文件修改有问题
                    _log.Information($"在InputIOName枚举中未找到与IO配置文件相匹配的IO，请检查");
                    //throw new Exception($"在InputIOName枚举中未找到与IO配置文件相匹配的IO，请检查");
                    _config.IOStateSourceConfig.States.Add(new IOStateConfig()
                    {
                        Name = io.ToString(),
                        Type = IOType.Input,
                        ByteIndex = bitIndex / 8,
                        BitIndex = bitIndex % 8,
                        ActiveLevel = IOActiveLevel.Lower
                    });
                }
                bitIndex++;
            }


            bitIndex = 12;//11个输出已占用
            var outputIONames = Enum.GetValues(typeof(OutputIOName)).Cast<OutputIOName>();
            foreach (var io in outputIONames)
            {
                if (!_config.IOStateSourceConfig.States.Any(o => o.Name == io.ToString()))
                {
                    //防止IO文件修改有问题
                    _log.Information($"在OutputIOName枚举中未找到与IO配置文件相匹配的IO，请检查");
                    _config.IOStateSourceConfig.States.Add(new IOStateConfig()
                    {
                        Name = io.ToString(),
                        Type = IOType.Output,
                        ByteIndex = bitIndex / 8,
                        BitIndex = bitIndex % 8,
                        ActiveLevel = IOActiveLevel.High
                    }); ;
                }
                bitIndex++;
            }         
        }

        private void Initial()
        {
            InputIOs = new ObservableCollection<StateValue>();
            OutputIOs = new ObservableCollection<StateValue>();

            foreach (var state in _ioSource.InputStates)
            {
                InputIOs.Add(new StateValue(state.Value));
            }
           
            foreach (var state in _ioSource.OutputStates)
            {
                    OutputIOs.Add(new StateValue(state.Value));
            }
        }

        public void Connect()
        {
            if (!_trioControl.IsConnected)
            {
                _trioControl.Connect();      
            }
            _ioSource.Connect();
        }

        public void SetValue(string name)
        {
            if (_ioSource.OutputStates.ContainsKey(name))
            {
                var state = _ioSource.OutputStates[name];
                var flag = !state.State;
                state.Set(flag);
                var output = OutputIOs.Where(o => o.Name == name).FirstOrDefault();
                output.IsOn = flag;
            }
        }

        public IOState GetInputIO(string name)
        {
            if (_ioSource.InputStates.ContainsKey(name))
            {
                return _ioSource.InputStates[name];
            }
            else
            {
                _log.Error($"不存在Name属性为{name}的IO");
                throw new Exception();
            }
        }

        public bool GetInputIOStatus(string ioName)
        {
            var io = InputIOs.Where(o => o.Name == ioName).FirstOrDefault();
            if (io == null)
            {
                throw new Exception("代码出错");
            }
            else
            {
                return io.IsOn;
            }
        }

        private void RefreshIOState()
        {
            if (_trioControl.IsConnected)
            {
                var states = _ioSource.InputStates;
                foreach (var stateValue in InputIOs)
                {
                    if (states.ContainsKey(stateValue.Name))
                        stateValue.IsOn = states[stateValue.Name].State;
                }

                foreach (var stateValue in OutputIOs)
                {
                    if (states.ContainsKey(stateValue.Name))
                        stateValue.IsOn = states[stateValue.Name].State;
                }
            }           
        }
    }

    public class IOManagerConfig:NotifyPropertyChanged
    {
        public IOStateSourceConfig IOStateSourceConfig { get; set; } = new IOStateSourceConfig()
        {
            Name = "IOManager"
        };
    }

    public class StateValue : NotifyPropertyChanged
    {
        private string _name ;
        private bool _isOn;
        private string _description;
        public StateValue(IOState ioState)
        {
            _name = ioState.Name;
            _isOn = ioState.State;
            _description = ioState.Description;
        }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
       
        public bool IsOn
        {
            get => _isOn;
            set => SetProperty(ref _isOn, value);
        }
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
    }


    public enum InputIOName
    {
        CCDZPositiveLimit,  //CCD-Z正限位
        CCDZNegativeLimit,  //CCD-Z负限位
        MaskZPositiveLimt,  //Mask-Z正限位
        MaskZNegativeLimit, //Mask-Z正限位
        GlueZPositiveLimit, //Glue-Z正限位
        GlueZNegativeLimit, //Glue-Z正限位
        AfmXPositiveLimit,  //Afm-X正限位
        AfmXNegativeLimit,  //Afm-X正限位
        AfmYPositiveLimit,  //Afm-Y正限位
        AfmYNegativeLimit,  //Afm-Y正限位
        UVXPositiveLimit,   //UV-X正限位
        UVXNegativeLimit,   //UV-X正限位
        AfmZPositiveLimit,  //Afm-Z正限位
        Empty0,             //
        Empty1,         //空
        Empty2,        //空
        Start,        //开始按钮In16
        Stop,         //关闭按钮In17
        GoHome,       //回零按钮In18
        OpenVacuum,   //打开真空阀吸气In19
        CloseVacuum,  //关闭真空阀吹气In20
        Emergency,    //急停按钮In21
        HasWafe       //真空检测In22
        
    }
    public enum OutputIOName
    {
        StartLight,        //开始按钮灯out0
        StopLight,         //关闭按钮灯out1
        GoHomeLight,       //回零按钮灯out2
        OpenVacuumLight,   //打开真空阀吸气out3
        CloseVacuumLight,  //打开真空阀吹气out4
        OpenVacuumControl, //打开吸气真空阀out5
        CloseVacuumControl,//打开吹气真空阀out6
        RedLight,     //三色灯红灯out7
        OrangeLight,  //三色灯黄灯out8
        GreenLight,   //三色灯绿灯out9
        BlueLight,    //三色灯蓝灯out10
        Buzzer,       //三色灯蜂鸣器out11
        Lighting,     //设备照明
        Relay,        //备用继电器
    }
}
