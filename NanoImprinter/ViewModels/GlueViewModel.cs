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
using WestLakeShape.Motion;

namespace NanoImprinter.ViewModels
{
    public class GlueViewModel : BindableBase
    {
        private readonly IDeviceManager _deviceManager;
        private GluePlatform _gluePlatform;
        private GluePlatformConfig _platformConfig;

        private string _portName;
        private bool _isReady;       //Z轴是否Ready
        private bool _isAlaram;      //Z轴是否报警

        private int _openTime;        //开阀时间
        private int _closeTime;       //关阀时间
        private int _openIntensity;   //开阀力度
        private int _temperature;     //当前温度
        private double _waitPosition;
        private double _gluePosition;
        private double _workVel;


        #region property
        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        public bool IsReady 
        {
            get => _isReady; 
            set => SetProperty(ref _isReady, value); 
        }

        public bool IsAlarm
        {
            get => _isAlaram;
            set => SetProperty(ref _isAlaram, value);
        }
        
        public double WaitPosition
        {
            get => _waitPosition;
            set => SetProperty(ref _waitPosition, value);
        }
        public double GluePosition
        {
            get => _gluePosition;
            set => SetProperty(ref _gluePosition, value);
        }
        public double WorkVel 
        {
            get => _workVel;
            set => SetProperty(ref _workVel, value);
        }


        public int OpenTime
        {
            get => _openTime;
            set => SetProperty(ref _openTime, value);
        }
        public int CloseTime
        {
            get => _closeTime;
            set => SetProperty(ref _closeTime, value);
        }

        public int OpenIntensity
        {
            get => _openIntensity;
            set => SetProperty(ref _openIntensity, value);
        }

        public int Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        public ObservableCollection<IAxis> Axes { get; set; }
        public ObservableCollection<string> PortNames{ get; set; }
        #endregion

        #region command
        public DelegateCommand GoHomeCommand => new DelegateCommand(GoHome).ObservesCanExecute(() => IsReady);
        public DelegateCommand ClearAlarmCommand => new DelegateCommand(ResetAlarm).ObservesCanExecute(() => IsAlarm);
        public DelegateCommand MoveToWaitPositionCommand => new DelegateCommand(MoveToWaitPosition).ObservesCanExecute(() => IsReady);
        public DelegateCommand MoveToGluePositionCommand => new DelegateCommand(MoveToTakePicturePosition).ObservesCanExecute(() => IsReady);
        public DelegateCommand SaveParamCommand => new DelegateCommand(SaveParam);
        public DelegateCommand ReloadParamCommand => new DelegateCommand(ReloadParam);
        public DelegateCommand GlueCommand => new DelegateCommand(Glue);
        public DelegateCommand RefreshPortNamesCommand => new DelegateCommand(RefreshPortNames);
        public DelegateCommand ConnectedCommand => new DelegateCommand(Connected);

        #endregion

        public GlueViewModel(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
            _gluePlatform = _deviceManager.GetPlatform(typeof(GluePlatform).Name) as GluePlatform;
            _platformConfig = _deviceManager.Config.GluePlatform;
            Axes = new ObservableCollection<IAxis>();
            PortNames = new ObservableCollection<string>();
            Axes.Add(_gluePlatform.GlueZAxis);
            RefreshPortNames(); 
            ReloadParam();

            _isReady = true;
        }


        private void GoHome()
        {
            var task = Task.Run(() =>
            {
                _gluePlatform.GoHome();
            });
        }
        private void ResetAlarm()
        {
           
            //_gluePlatform.ResetAxesAlarm();
        }

        private void MoveToWaitPosition()
        {
            var task = Task.Run(() =>
            {
                _gluePlatform.MoveToWaitPosition();
            });
        }

        private void MoveToTakePicturePosition()
        {
            var task = Task.Run(() =>
            {
                _gluePlatform.MoveToGluePosition();
            });
        }

        private void SaveParam()
        {
            _platformConfig.WaitPosition = WaitPosition;
            _platformConfig.GluePosition = GluePosition;
            _platformConfig.WorkVel = WorkVel;
            _platformConfig.GlueConfig.PortName = PortName;
            _platformConfig.GlueConfig.OpenValveTime = OpenTime;
            _platformConfig.GlueConfig.OpenValveIntensity = OpenIntensity;
            _platformConfig.GlueConfig.ClosedValveTime = CloseTime;
            _platformConfig.GlueConfig.TargetTemperatore = Temperature;
            _deviceManager.SaveParam();
            _gluePlatform.ReloadConfig();
        }
   
        private void ReloadParam()
        {
            WaitPosition = _platformConfig.WaitPosition;
            GluePosition = _platformConfig.GluePosition;
            WorkVel = _platformConfig.WorkVel;
            PortName = _platformConfig.GlueConfig.PortName;
            OpenTime = _platformConfig.GlueConfig.OpenValveTime;
            OpenIntensity = _platformConfig.GlueConfig.OpenValveIntensity;
            CloseTime = _platformConfig.GlueConfig.ClosedValveTime;
            Temperature = _platformConfig.GlueConfig.TargetTemperatore;
        }

        private void Glue()
        {
            _gluePlatform.Glue();
        }

        private void RefreshPortNames()
        {
            PortNames.Clear();
            foreach (var port in SerialPort.GetPortNames())
            {
                PortNames.Add(port);
            }
        }
        private void Connected()
        {
            _gluePlatform.Connected();
        }
    }
}
