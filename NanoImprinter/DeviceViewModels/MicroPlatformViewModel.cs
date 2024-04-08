using NanoImprinter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WestLakeShape.Common;
using WestLakeShape.Common.WpfCommon;

namespace NanoImprinter.DeviceViewModels
{
    public class MicroPlatformViewModel : NotifyPropertyChanged
    {

        private MicroPlatform _device;
        private PointZRXY _currentPosition;

        public double ZPosition
        {
            get => _currentPosition.Z;
            set
            {
                if (_currentPosition.Z != value)
                {
                    _currentPosition = new PointZRXY(value, _currentPosition.RX, _currentPosition.RY);
                    OnPropertyChanged();
                }
            }
        }
        public double RXPosition
        {
            get => _currentPosition.RX;
            set
            {
                if (_currentPosition.RX != value)
                {
                    _currentPosition = new PointZRXY(_currentPosition.Z, value, _currentPosition.RY);
                    OnPropertyChanged();
                }
            }
        }
        public double RYPosition
        {
            get => _currentPosition.RY;
            set
            {
                if (_currentPosition.RY != value)
                {
                    _currentPosition = new PointZRXY(_currentPosition.Z, _currentPosition.RX, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsConnected
        {
            get => _device.IsConnected;
        }

        #region  Command
        public ICommand GoHomeCommand { get; private set; }
        public ICommand ConnectedCommand { get; private set; }
        public ICommand DisconnectedCommand { get; private set; }
        public ICommand ImprintCommand { get; private set; }
        public ICommand DemoldCommand { get; private set; }
        public ICommand CreepCommand { get; private set; }
        public ICommand JogForwardCommand { get; private set; }
        public ICommand JogBackCommand { get; private set; }
        #endregion

        public MicroPlatformViewModel(MicroPlatform device)
        {
            _device = device;
            ConnectedCommand = new RelayCommand(_device.Connected);
            //GoHomeCommand = new RelayCommand(_device.GoHome);
            //DisconnectedCommand = new AsyncRelayCommand(_device.OnDisconnecting);
            //ImprintCommand = new AsyncRelayCommand(_device.Imprint);
            //DemoldCommand = new AsyncRelayCommand(_device.Demold);
            //CreepCommand = new AsyncRelayCommand(_device.Creep);
            //JogForwardCommand = new AsyncRelayCommand(_device.JogForward);
            //JogBackCommand = new AsyncRelayCommand(_device.JogBackward);
        }

    }
}
