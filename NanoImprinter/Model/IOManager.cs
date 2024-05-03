﻿using Serilog;
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
        
        public IOManagerConfig Config => _config;
        public ObservableCollection<StateValue> InputIOs { get; set; }
        public ObservableCollection<StateValue> OutputIOs { get; set; }

        public IOManager(IOManagerConfig config)
        {
            _config = config;

            LoadIOStateSourceConfig();
            //根据io配置创建IOState
            _ioSource = new TrioIOStateSource(_config.IOStateSourceConfig);

            Initial();

            RefreshDataService.Instance.Register(RefreshIOState);
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

        /// <summary>
        /// 完成IO配置
        /// </summary>
        private void LoadIOStateSourceConfig()
        {
            int bitIndex = 13;//轴限位开关占用了13个inputIO
            var inputIOs = Enum.GetValues(typeof(InputIOName)).Cast<InputIOName>();
            
            foreach (var io in inputIOs)
            {
                if (!_config.IOStateSourceConfig.States.Any(o => o.Name == io.ToString()))
                {
                    //防止IO文件修改有问题
                    _log.Information($"在InputIOName枚举中未找到与IO配置文件相匹配的IO，请检查");
                    _config.IOStateSourceConfig.States.Add(new IOStateConfig()
                    {
                        Name = io.ToString(),
                        Type = IOType.Input,
                        ByteIndex = bitIndex / 8,
                        BitIndex = bitIndex % 8,
                        ActiveLevel = IOActiveLevel.Lower
                    }); ;
                }
                bitIndex++;
            }

            bitIndex = 0;
            var outputIOs = Enum.GetValues(typeof(OutputIOName)).Cast<OutputIOName>();
            
            foreach (var io in outputIOs)
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
                        ActiveLevel = IOActiveLevel.Lower
                    }); ;
                }
                bitIndex++;
            }
            _config.IOStateSourceConfig.InputBufferLength = inputIOs.Count();
            _config.IOStateSourceConfig.OutputBufferLength = outputIOs.Count();
       }

        private void Initial()
        {
            InputIOs = new ObservableCollection<StateValue>();
            OutputIOs = new ObservableCollection<StateValue>();

            var states = _ioSource.InputStates;

            foreach (var name in Enum.GetValues(typeof(InputIOName)))
            {
                if (states.ContainsKey(name.ToString()))
                    InputIOs.Add(new StateValue(states[name.ToString()]));
            }

            foreach (var name in Enum.GetValues(typeof(InputIOName)))
            {
                if (states.ContainsKey(name.ToString()))
                    OutputIOs.Add(new StateValue(states[name.ToString()]));
            }
        }

        private void RefreshIOState()
        {
            var states = _ioSource.InputStates;
            foreach (var stateValue in InputIOs)
            {
               if(states.ContainsKey(stateValue.Name))
                stateValue.IsOn = states[stateValue.Name].State;
            }

            foreach (var stateValue in OutputIOs)
            {
                if (states.ContainsKey(stateValue.Name))
                    stateValue.IsOn = states[stateValue.Name].State;
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
        Start,
        Stop,
        GoHome,
        Reset,
        Emergency,
        SaftDoor,
        LoadWafeDoor,
        HasWafe,
    }
    public enum OutputIOName
    {
        StartLight,
        GoHomeLight,
        ResetLight,
        EmergencyLight,
        FixedMark,
        FixedWafe,
        OpenAirControl,
        ClosedAirControl
    }
}