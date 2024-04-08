using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WestLakeShape.Common;
using WestLakeShape.Common.WpfCommon;
using WestLakeShape.Motion.Device;

namespace NanoImprinter.Model
{
    public class MicroPlatformAsync
    {
        private MicroPlatformAsyncConfig _config;
        private PiezoActuator _piezo;
        private bool _isconnected;
        private PointZRXY _currentPosition;

        public bool IsConnected => _piezo.IsConnected;

        public MicroPlatformAsyncConfig Config
        {
            get => _config;
            set => _config = value;
        }

        public PointZRXY CurrentPosition => _currentPosition;

        public MicroPlatformAsync(MicroPlatformAsyncConfig config)
        {
            _config = config;
            //_piezo = new PiezoActuator(_config.ComName);
            _currentPosition = new PointZRXY(0, 0, 0);
        }


        public void OnConnecting()
        {
            if (_isconnected)
            {
                _piezo.Connect();
                Thread.Sleep(100);
                _currentPosition = RefreshStates();
            }
        }

        public void OnDisconnecting()
        {
            if (!_isconnected)
                _piezo.Disconnected();

        }

        /// <summary>
        /// 微平台压印流程
        /// </summary>
        /// <returns></returns>
        public Task<bool> Imprint()
        {
            MoveTo(ChannelNo.Third, _config.PreprintPosition);
            return Task.FromResult(true);
        }

        /// <summary>
        /// 微平台脱模流程
        /// </summary>
        /// <returns></returns>
        public Task<bool> Demold()
        {
            MoveTo(_config.DemoldPosition);

            return Task.FromResult(true);
        }

        /// <summary>
        /// 轴缓慢移动
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task<bool> Creep(ChannelNo index)
        {
            _piezo.WriteDisplace(index, _config.CreepDistance);
            return Task.FromResult(true);
        }

        /// <summary>
        /// 回零
        /// </summary>
        /// <returns></returns>
        public Task<bool> GoHome()
        {
            var zero = new PointZRXY(0, 0, 0);
            MoveTo(zero);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Jog前移
        /// </summary>
        /// <param name="index"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public Task<bool> JogForward(ChannelNo index, double position)
        {
            var targetPosition = CalculateDisplace(index, position);
            MoveTo(index, targetPosition);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Jog后移
        /// </summary>
        /// <param name="index"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public Task<bool> JogBackward(ChannelNo index, double position)
        {
            var targetPosition = CalculateDisplace(index, -position);
            MoveTo(index, targetPosition);

            return Task.FromResult(true);
        }

        /// <summary>
        /// 单通道移动
        /// </summary>
        /// <param name="index"></param>
        /// <param name="position"></param>
        private void MoveTo(ChannelNo index, double position)
        {
            _piezo.WriteDisplace(index, position);
            Thread.Sleep(100);

            RefreshStates();
        }

        /// <summary>
        /// 多通道移动
        /// </summary>
        /// <param name="position"></param>
        private void MoveTo(PointZRXY position)
        {
            _piezo.WriteMultiDisplace(new double[] { position.RX, position.RY, position.Z });
            Thread.Sleep(100);

            RefreshStates();
        }


        private PointZRXY RefreshStates()
        {
            var position = _piezo.ReadMultiDisplace();

            return new PointZRXY(position[0], position[1], position[2]);
        }

        private double CalculateDisplace(ChannelNo index, double postion)
        {
            double position = 0;
            switch (index)
            {
                case ChannelNo.One:
                    position = _currentPosition.RX - postion;
                    break;
                case ChannelNo.Two:

                    position = _currentPosition.RY - postion;
                    break;
                case ChannelNo.Third:
                    position = _currentPosition.Z - postion;
                    break;
            }

            return position;
        }

    }
    public class MicroPlatformAsyncConfig:NotifyPropertyChanged
    {
        private string _comName;
        private double _prepintPosition;
        private double _creepDistance;
        private PointZRXY _preparePosition;
        private PointZRXY _demoldPosition;

        [Category("MicroPlatform"), Description("串口名称")]
        [DisplayName("串口名称")]
        public string ComName 
        {
            get => _comName;
            set => SetProperty(ref _comName, value);
        }

        [Category("MicroPlatform"), Description("预压印位置")]
        [DisplayName("预压印位置")]
        public double PreprintPosition 
        {
            get => _prepintPosition;
            set => SetProperty(ref _prepintPosition, value);
        }

        [Category("MicroPlatform"), Description("蠕动距离")]
        [DisplayName("蠕动距离")]
        public double CreepDistance
        {
            get => _creepDistance;
            set => SetProperty(ref _creepDistance, value);
        }

        [Category("MicroPlatform"), Description("脱模位置")]
        [DisplayName("脱模位置")]
        public PointZRXY DemoldPosition
        {
            get => _demoldPosition;
            set => SetProperty(ref _demoldPosition, value);
        }

        [Category("MicroPlatform"), Description("准备位置")]
        [DisplayName("准备位置")]
        public PointZRXY PreparePosition
        {
            get => _preparePosition;
            set => SetProperty(ref _preparePosition, value);
        }
    }
}
