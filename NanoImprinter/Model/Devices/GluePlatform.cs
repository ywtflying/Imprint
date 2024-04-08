using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WestLakeShape.Common.WpfCommon;
using WestLakeShape.Motion;
using WestLakeShape.Motion.Device;

namespace NanoImprinter.Model
{
    public interface IGluePlatform : INotifyPropertyChanged, IPlatform
    {
        GluePlatformConfig Config { get; set; }
        //bool GoHome();
        bool MoveToWaitPosition();
        bool MoveToGluePosition();
        bool Glue();
        void ResetAxesAlarm();
    }

    public class GluePlatform : IGluePlatform
    {
        private GluePlatformConfig _config;
        private IAxis _glueZAxis;
        private GlueControl _glueControl;
        private double _currentPositionGlueZ;

        public IAxis GlueZAxis => _glueZAxis;
        public bool IsConnected => _glueControl.IsConnected;

        public GluePlatformConfig Config
        {
            get => _config;
            set => _config = value;
        }

        public double CurrentPositionGlueZ
        {
            get => _currentPositionGlueZ;
            set
            {
                if (_currentPositionGlueZ != value)
                {
                    _currentPositionGlueZ = value;
                    OnPropertyChanged(nameof(CurrentPositionGlueZ));
                }
            }
        }

        public GluePlatform(GluePlatformConfig config,IAxis[] axes)
        {
            _config = config;
            //_zAxis = new TrioAxis(config.AxisConfig);
            _glueZAxis = axes[0];
            _glueControl = new GlueControl(config.GlueConfig);

        }
        public void ReloadConfig()
        {
            _glueControl.ReloadConfig();
        }

        private void LoadAxesVelocity()
        {
            _glueZAxis.LoadVelocity(Config.WorkVel);
        }

        /// <summary>
        /// 点胶回零与AFM相关；点胶》AFM
        /// </summary>
        /// <returns></returns>
        public bool GoHome()
        {
            return _glueZAxis.GoHome();
        }

        public bool MoveToWaitPosition()
        {
            return MoveBy(_config.WaitPosition);
        }
        public bool MoveToGluePosition()
        {
            return MoveBy(_config.GluePosition);
        }

        private bool MoveBy(double position)
        {
            _glueZAxis.MoveTo(position);
            return true;
        }

        /// <summary>
        /// 点胶
        /// </summary>
        /// <returns></returns>
        public bool Glue()
        {
            _glueControl.StartDispense();
            return true;
        }

        public void ResetAxesAlarm()
        {
            ((TrioAxis)_glueZAxis).ResetAlarm();
        }

        private void RefreshRealtimeData()
        {
            if (_currentPositionGlueZ != _glueZAxis.Position)
                CurrentPositionGlueZ = _glueZAxis.Position;
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

        public void Connected()
        {
            _glueControl.Connect();
        }

        public void Disconnected()
        {
            _glueControl.Disconnected();
        }
    }


    public class GluePlatformConfig: NotifyPropertyChanged
    {
        private double _gluePosition;
        private double _waitPosition;
        private double _workVel;
        private GlueControlConfig _glueConfig = new GlueControlConfig() ;

        [Category("GluePlatform"), Description("点胶高度")]
        [DisplayName("点胶高度")]
        public double GluePosition 
        { 
            get => _gluePosition;
            set => SetProperty(ref _gluePosition, value);
        }


        [Category("GluePlatform"), Description("等待点胶高度")]
        [DisplayName("等待高度")]
        public double WaitPosition
        {
            get => _waitPosition;
            set => SetProperty(ref _waitPosition, value);
        }

        [Category("GluePlatform"), Description("运行速度")]
        [DisplayName("运行速度")]
        public double WorkVel
        {
            get => _workVel;
            set => SetProperty(ref _workVel, value);
        }

        [Category("GluePlatform"), Description("点胶控制器配置参数")]
        [DisplayName("点胶配置参数")]
        public GlueControlConfig GlueConfig 
        {
            get => _glueConfig;
            set => SetProperty(ref _glueConfig, value);
        }

        //[Category("GluePlatform"), Description("Z轴配置参数")]
        //[DisplayName("Z轴配置参数")]
        //public TrioAxisConfig AxisConfig { get; set; }
    }
}
