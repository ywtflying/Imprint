using NanoImprinter.Model;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WestLakeShape.Common;
using WestLakeShape.Motion.Device;

namespace NanoImprinter.ViewModels
{
    public class MicroViewModel : BindableBase
    {
        private readonly IDeviceManager _deviceManager;
        private MicroPlatform _microPlatform;
        private MicroPlatformConfig _microPlatformConfig;
        private double _currentPressure;
        private double _moveDistance;

        private ChannelNo _selectedChannel;//选择通道
        private string _portName;
        private double _contactPosition;
        private double _zCreepDistance;
        private double _demoldPositionZ;
        private double _demoldPositionRX;
        private double _demoldPositionRY;
        private double _levelPositionZ;
        private double _levelPositionRX;
        private double _levelPositionRY;
        private double _maxPressure;
        private double _minPressure;
        private bool _isClosedLoop;
        #region property

        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        public bool IsClosedLoop
        {
            get => _isClosedLoop;
            set => SetProperty(ref _isClosedLoop, value);
        }

        /// <summary>
        /// 压印过程快速移动到接触位，然后开始读取压力值并开始蠕动
        /// </summary>
        public double ContactPosition
        {
            get => _contactPosition;
            set => SetProperty(ref _contactPosition, value);

        }
        /// <summary>
        /// Z向压印过程中的蠕动距离
        /// </summary>
        public double ZCreepDistance
        {
            get => _zCreepDistance;
            set => SetProperty(ref _zCreepDistance, value);
        }

        public double DemoldPositionZ
        {
            get => _demoldPositionZ;
            set => SetProperty(ref _demoldPositionZ, value);
        }
        public double DemoldPositionRX
        {
            get => _demoldPositionRX;
            set => SetProperty(ref _demoldPositionRX, value);
        }
        public double DemoldPositionRY
        {
            get => _demoldPositionRY;
            set => SetProperty(ref _demoldPositionRY, value);
        }
        public double LevelPositionZ
        {
            get => _levelPositionZ;
            set => SetProperty(ref _levelPositionZ, value);
        }
        public double LevelPositionRX
        {
            get => _levelPositionRX;
            set => SetProperty(ref _levelPositionRX, value);
        }
        public double LevelPositionRY
        {
            get => _levelPositionRY;
            set => SetProperty(ref _levelPositionRY, value);
        }

        public double MaxPressure
        {
            get => _maxPressure;
            set => SetProperty(ref _maxPressure, value);
        }
        public double MinPressure
        {
            get => _minPressure;
            set => SetProperty(ref _minPressure, value);
        }

        public double CurrentPressure
        {
            get => _currentPressure;
            set => SetProperty(ref _currentPressure, value);
        }

        public double MoveDistance
        {
            get => _moveDistance;
            set => SetProperty(ref _moveDistance, value);
        }

        public ChannelNo SelectedChannel
        {
            get => _selectedChannel;
            set => SetProperty(ref _selectedChannel, value);
        }

        public IList<ChannelNo> ChannelIndex { get; }

        public ObservableCollection<string> PortNames { get; set; }
        #endregion


        #region Command

        public DelegateCommand GoHomeCommand => new DelegateCommand(GoHome);
        public DelegateCommand MoveToLevelPositionCommand => new DelegateCommand(MoveToLevelPosition);
        public DelegateCommand MoveToDemoldPositionCommand => new DelegateCommand(MoveToDemoldPosition);
        public DelegateCommand CreepCommand => new DelegateCommand(Creep);
        public DelegateCommand JogForwardCommand => new DelegateCommand(JogForward);
        public DelegateCommand JogBackwardCommand => new DelegateCommand(JogBackward);
        public DelegateCommand SaveParamCommand => new DelegateCommand(SaveParam);
        public DelegateCommand ReloadParamCommand => new DelegateCommand(ReloadParam);
        public DelegateCommand RefreshPortNamesCommand => new DelegateCommand(RefreshPortNames);
        public DelegateCommand ConnectedCommand => new DelegateCommand(Connected);
        public DelegateCommand ChangedLoopCommand => new DelegateCommand(ChangedLoop);
        #endregion



        public MicroViewModel(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
            _microPlatform = _deviceManager.GetPlatform(typeof(MicroPlatform).Name) as MicroPlatform;
            _microPlatform.OnMessage += ShowMessage;
            _microPlatformConfig = _deviceManager.Config.MicroPlatform;
            ChannelIndex = Enum.GetValues(typeof(ChannelNo)).Cast<ChannelNo>().ToList();
            PortNames = new ObservableCollection<string>(SerialPort.GetPortNames());
            RefreshPortNames();
            ReloadParam();
        }


        private void GoHome()
        {
           Task.Run(() => _microPlatform.GoHome());
        }

        private void MoveToLevelPosition()
        {
            Task.Run(() => _microPlatform.MoveTo(new PointZRXY(_levelPositionZ, _levelPositionRX, _levelPositionRY)));
        }

        private void MoveToDemoldPosition()
        {
            Task.Run(() => _microPlatform.MoveTo(new PointZRXY(_demoldPositionZ, _demoldPositionRX, _demoldPositionRY)));
        }

        private void Creep()
        {
            Task.Run(() => _microPlatform.Creep(SelectedChannel, MoveDistance));
        }

        private void JogForward()
        {
            Task.Run(() => _microPlatform.JogForward(SelectedChannel, MoveDistance));
        }
        private void JogBackward()
        {
            Task.Run(() => _microPlatform.JogBackward(SelectedChannel, MoveDistance));
        }

        private void RefreshPortNames()
        {
            PortNames.Clear();
            foreach (var port in SerialPort.GetPortNames())
            {
                PortNames.Add(port);
            }
        }

        private void SaveParam()
        {
            _microPlatformConfig.PiezoActuatorConfig.PortName = _portName;
            _microPlatformConfig.ContactPosition = ContactPosition;
            _microPlatformConfig.ZCreepDistance = ZCreepDistance;
            _microPlatformConfig.DemoldPosition = new PointZRXY(_demoldPositionZ, _demoldPositionRX, _demoldPositionRY);
            _microPlatformConfig.LevelPosition = new PointZRXY(_levelPositionZ, _levelPositionRX, _levelPositionRY);
            _microPlatformConfig.MaxPressure = MaxPressure;
            _microPlatformConfig.MinPressure = MinPressure;
            _deviceManager.SaveParam();
            _microPlatform.ReloadConfig();
        }
        private void ReloadParam()
        {
            PortName = _microPlatformConfig.PiezoActuatorConfig.PortName;
            ContactPosition = _microPlatformConfig.ContactPosition;
            ZCreepDistance = _microPlatformConfig.ZCreepDistance;
            DemoldPositionZ = _microPlatformConfig.DemoldPosition.Z;
            DemoldPositionRX = _microPlatformConfig.DemoldPosition.RX;
            DemoldPositionRY = _microPlatformConfig.DemoldPosition.RY;
            LevelPositionZ = _microPlatformConfig.LevelPosition.Z;
            LevelPositionRX = _microPlatformConfig.LevelPosition.RX;
            LevelPositionRY = _microPlatformConfig.LevelPosition.RY;
            MaxPressure = _microPlatformConfig.MaxPressure;
            MinPressure = _microPlatformConfig.MinPressure;
        }

        private void ChangedLoop()
        {
            _microPlatform.SetClosedLoop(!_isClosedLoop);
            IsClosedLoop = _microPlatform.IsClosedLoop;
        }
        private void Connected()
        {
            _microPlatform.Connected();
        }
        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
