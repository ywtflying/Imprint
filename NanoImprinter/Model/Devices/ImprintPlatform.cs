using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WestLakeShape.Common;
using WestLakeShape.Common.WpfCommon;
using WestLakeShape.Motion;
using WestLakeShape.Motion.Device;

namespace NanoImprinter.Model
{
    public interface IImprintPlatform : IPlatform
    {
        ImprintPlatformConfig Config { get; set; }
        //bool GoHome();
        bool MoveToMaskPreprintHeight();
        bool MoveToTakePictureHeight();
        void ResetAxesAlarm();
    }

    public class ImprintPlatform : NotifyPropertyChanged, IImprintPlatform
    {
        private ImprintPlatformConfig _config;
        private IAxis _maskZAxis;
        private IAxis _cameraZAxis;
        private IAxis _uvXAxis;
        private ForceSensorControl _forceSensorControl;
        private UVControl _uvControl;
        private bool _isHomeComplete;

        private double _currentPositionMaskZ;  //掩膜Z轴当前位置
        private double _currentPositionCameraZ;
        private double _currentPositionUVX;
        private double _forceValue0;//压力传感器1的力值
        private double _forceValue1;
        private double _forceValue2;
        private double _forceValue3;

        public ImprintPlatformConfig Config
        {
            get => _config;
            set => _config = value;
        }

        public IAxis MaskZAxis => _maskZAxis;
        public IAxis CameraZAxis => _cameraZAxis;
        public IAxis UVXAxis => _uvXAxis;
        public bool IsConnected => _uvControl.IsConnected;

        public bool IsHomeComplete => _isHomeComplete;

        #region 实时数据
        public double CurrentPositionMaskZ
        {
            get => _currentPositionMaskZ;
            set
            {
                var val = Math.Round(value, 6);
                SetProperty(ref _currentPositionMaskZ, val);
            }
        }

        public double CurrentPositionCameraZ
        {
            get => _currentPositionCameraZ;
            set
            {
                var val = Math.Round(value, 6);
                SetProperty(ref _currentPositionCameraZ, val);
            }
        }

        public double CurrentPositionUVX
        {
            get => _currentPositionUVX;
            set
            {
                var val = Math.Round(value, 6);
                SetProperty(ref _currentPositionUVX, val);
            }
        }

        public double ForceValue0
        {
            get => _forceValue0;
            set
            {
                var val = Math.Round(value, 3);
                SetProperty(ref _forceValue0, val);
            }
        }
        public double ForceValue1
        {
            get => _forceValue1;
            set
            {
                var val = Math.Round(value, 3);
                SetProperty(ref _forceValue1, val);
            }
        }
        public double ForceValue2
        {
            get => _forceValue2;
            set
            {
                var val = Math.Round(value, 3);
                SetProperty(ref _forceValue2, val);
            }
        }
        public double ForceValue3
        {
            get => _forceValue3;
            set
            {
                var val = Math.Round(value, 3);
                SetProperty(ref _forceValue3, val);
            }
        }

        public string Name => throw new NotImplementedException();

        #endregion

        public ImprintPlatform(ImprintPlatformConfig config,IAxis[] axes)
        {
            _config = config;
            _maskZAxis = axes[0];
            _cameraZAxis = axes[1];
            _uvXAxis = axes[2];
            _forceSensorControl = new ForceSensorControl(_config.ForceSensorControlConfig);
            _uvControl = new UVControl(_config.UVConfig);
            RefreshDataService.Instance.Register(RefreshRealtimeData);
        }

        public void Connect()
        {
            _forceSensorControl.Connected();
            _uvControl.OnConnecting();
        }
        public void ConnectedForceControl()
        {
            _forceSensorControl.Connected();
        }
        public void ConnectedUVControl()
        {
            _uvControl.OnConnecting();
        }
        public void Disconnect()
        {
            _forceSensorControl.Disconnected();
            _uvControl.OnDisconnecting();
        }

        /// <summary>
        /// 依次回零顺序：UV》相机》压印
        /// </summary>
        /// <returns></returns>
        public bool GoHome()
        {
            _isHomeComplete = false;
            //var uvxTask = Task.Run(()=>_uvXAxis.GoHome());
            //Task.WaitAll(uvxTask);
            var cZTask = Task.Run(() => _cameraZAxis.GoHome());
            //Task.WaitAll(cZTask);
            //return _maskZAxis.GoHome();
            _isHomeComplete = true;
            return _isHomeComplete;
            
        }

        public void ReloadConfig()
        {
            _uvControl.ReloadConfig();
            _forceSensorControl.ReloadConfig();
        }
        public void LoadAxesVelocity()
        {
            _uvXAxis.LoadVelocity(Config.UVXWorkVel);
            _cameraZAxis.LoadVelocity(Config.CameraZWorkVel);
            _maskZAxis.LoadVelocity(Config.MaskZWorkVel);
        }

        public bool MoveToMaskPreprintHeight()
        {
            if (_uvXAxis.Position <= _config.UVWaitPosition)
                throw new Exception("UV未离开冲突区域，移动相机会发生碰撞");

            return MoveBy(_maskZAxis,_config.MaskPreprintHeight);
        }
        public bool MoveToMaskWaitHeight()
        {
            //计算压头目标位置是否与相机存在碰撞可能
            var safePosition = Math.Abs(_config.MaskWaitHeight) - Math.Abs(_config.SafeDistanceOfCameraAndMask);
            //if (Math.Abs(_cameraZAxis.Position) < safePosition)
            //    throw new Exception("相机当前位置太低，移动压印头会发生碰撞");

            return MoveBy(_maskZAxis, _config.MaskWaitHeight);
        }

        public bool MoveToTakePictureHeight()
        {
            //if (_uvXAxis.Position <= _config.UVWaitPosition)
            //    throw new Exception("UV未离开冲突区域，移动相机会发生碰撞");
            //计算相机目标位置是否与压头存在碰撞可能
            var safePosition = Math.Abs(_config.CameraTakePictureHeight) + Math.Abs(_config.SafeDistanceOfCameraAndMask);
            if (Math.Abs(_maskZAxis.Position) > safePosition)
                throw new Exception("压头高度太高，移动相机会发生碰撞");

            return MoveBy(_cameraZAxis,_config.CameraTakePictureHeight);
        }

        public bool MoveToCameraWaitHeight()
        {
            return MoveBy(_cameraZAxis,_config.CameraWaitHeight);
        }

        public bool MoveToUVWaitPositon()
        {
            var xTask = Task.Run(() => MoveBy(_uvXAxis, _config.UVWaitPosition));
            Task.WaitAll(xTask);
            return true;
        }

        public bool MoveToUVIrradiationPosition()
        {
            //相机和Mask的位置值为负值，
            //后期可根据情况修改成相机和压头轴必须在等待位。
            if (Math.Abs(_cameraZAxis.Position) < Math.Abs(_config.UVSafePositionForCamera))
                throw new Exception("相机高度太低，移动UV会发生碰撞");
            if (Math.Abs(_maskZAxis.Position) < Math.Abs(_config.UVSafePositionForMask))
                throw new Exception("掩膜高度太低，移动UV会发生碰撞");

            var xTask = Task.Run(() => MoveBy(_uvXAxis, _config.UVIrradiationPosition));
            Task.WaitAll(xTask);
            return true;
        }

        private bool MoveBy(IAxis axis,double position)
        {
            return axis.MoveTo(position);
        }

        public bool UVIrradiate()
        {
            _uvControl.Open(_uvControl.FirstChannel);
            Thread.Sleep(_config.UVConfig.IrradiationTime);
            return true;
        }

        public void ResetAxesAlarm()
        {
            ((TrioAxis)_maskZAxis).ResetAlarm();
            ((TrioAxis)_cameraZAxis).ResetAlarm();
        }

        public void OpenUVLight()
        {
            _uvControl.Open(_uvControl.FirstChannel);
        }
        public void CloseUVLight()
        {
            _uvControl.Close(_uvControl.FirstChannel);
        }

        public void WriteUVParam()
        {
            _uvControl.WriteIrradiationParameter();
        }

        public void ReadUVParam()
        {
            _uvControl.ReadIrradiationParameter();
        }

        private void RefreshRealtimeData()
        {
            //刷新轴当前位置
            CurrentPositionCameraZ = _cameraZAxis.Position;
            CurrentPositionMaskZ = _maskZAxis.Position;
            CurrentPositionUVX = _uvXAxis.Position;
            //刷新压力传感器值
            ForceValue0 = _forceSensorControl.ForceValue0;
            ForceValue1 = _forceSensorControl.ForceValue1;
            ForceValue2 = _forceSensorControl.ForceValue2;
            ForceValue3 = _forceSensorControl.ForceValue3;
        }
    }

    public class ImprintPlatformConfig : NotifyPropertyChanged
    {
        private double _maskWaitHeight;
        private double _maskPrepintHeight;
        private double _maskZWorkVel;
        private double _cameraWaitHeight;
        private double _cameraTakePictureHeight;
        private double _cameraZWorkVel;
        private double _uvWaitPosition ;
        private double _uvIrradiationPosition;
        private double _uvXWorkVel;
        private UVControlConfig _uvConfig = new UVControlConfig();
        private ForceSensorControlConfig _forceSensorConfig = new ForceSensorControlConfig();
        private double _uvSafePositionForCamera;
        private double _uvSafePositionForMask;
        private double _safeDistanceOfCameraAndMask;

        //掩膜组件
        [Category("ImprintPlatform"), Description("掩膜等待高度")]
        [DisplayName("掩膜等待高度")]
        public double MaskWaitHeight 
        {
            get => _maskWaitHeight;
            set => SetProperty(ref _maskWaitHeight, value);
        }

        //掩膜组件
        [Category("ImprintPlatform"), Description("掩膜预压印高度")]
        [DisplayName("掩膜预压印高度")]
        public double MaskPreprintHeight 
        {
            get => _maskPrepintHeight;
            set => SetProperty(ref _maskPrepintHeight, value);
        }

        [Category("ImprintPlatform"), Description("压印速度")]
        [DisplayName("压印速度")]
        public double MaskZWorkVel
        {
            get => _maskZWorkVel;
            set => SetProperty(ref _maskZWorkVel, value);
        }


        //拍照组件参数
        [Category("ImprintPlatform"), Description("等待拍照高度")]
        [DisplayName("等待拍照高度")]
        public double CameraWaitHeight
        {
            get => _cameraWaitHeight;
            set => SetProperty(ref _cameraWaitHeight, value);
        }

        [Category("ImprintPlatform"), Description("拍照高度")]
        [DisplayName("拍照高度")]
        public double CameraTakePictureHeight
        {
            get => _cameraTakePictureHeight;
            set => SetProperty(ref _cameraTakePictureHeight, value);
        }

        [Category("ImprintPlatform"), Description("移动拍照位速度")]
        [DisplayName("移动拍照位速度")]
        public double CameraZWorkVel
        {
            get => _cameraZWorkVel;
            set => SetProperty(ref _cameraZWorkVel, value);
        }
        [Category("ImprintPlatform"), Description("相机移动过程中，X方向UV发生碰撞的位置")]
        [DisplayName("X方向安全位置")]
        public double SafeDistanceOfCameraAndMask
        {
            get => _safeDistanceOfCameraAndMask;
            set => SetProperty(ref _safeDistanceOfCameraAndMask, value);
        }



        //UV组件参数
        [Category("ImprintPlatform"), Description("UV等待位")]
        [DisplayName("UV等待位")]
        public double UVWaitPosition 
        {
            get => _uvWaitPosition;
            set => SetProperty(ref _uvWaitPosition, value);
        }

        [Category("ImprintPlatform"), Description("UV照射位")]
        [DisplayName("UV照射位")]
        public double UVIrradiationPosition
        {
            get => _uvIrradiationPosition ;
            set => SetProperty(ref _uvIrradiationPosition, value);
        }

        [Category("ImprintPlatform"), Description("UVX轴工作速度")]
        [DisplayName("UVX轴工作速度")]
        public double UVXWorkVel 
        {
            get => _uvXWorkVel;
            set => SetProperty(ref _uvXWorkVel, value);
        }

        [Category("ImprintPlatform"), Description("UV配置参数")]
        [DisplayName("UV配置参数")]
        public UVControlConfig UVConfig
        {
            get => _uvConfig?? new UVControlConfig();
            set => SetProperty(ref _uvConfig, value);
        }
        [Category("ImprintPlatform"), Description("力传感器配置参数")]
        [DisplayName("力传感器参数")]
        public ForceSensorControlConfig ForceSensorControlConfig
        {
            get => _forceSensorConfig;
            set => SetProperty(ref _forceSensorConfig, value);
        }


        [Category("PrintPlatform"), Description("UV移动过程中，Z方向相机发生碰撞的位置")]
        [DisplayName("Z方向相机安全位置")]
        public double UVSafePositionForCamera
        {
            get => _uvSafePositionForCamera;
            set => SetProperty(ref _uvSafePositionForCamera, value);
        }
        [Category("PrintPlatform"), Description("UV移动过程中，Z方向掩膜发生碰撞的位置")]
        [DisplayName("Z方向相机安全位置")]
        public double UVSafePositionForMask
        {
            get => _uvSafePositionForMask;
            set => SetProperty(ref _uvSafePositionForMask, value);
        }
    }
}
