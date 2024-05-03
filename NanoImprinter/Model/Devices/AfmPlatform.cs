using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WestLakeShape.Common;
using WestLakeShape.Common.WpfCommon;
using WestLakeShape.Motion;

namespace NanoImprinter.Model
{
    public interface IAfmPlatform : IPlatform
    {
        void Run();
    }
    public class AfmPlatform : NotifyPropertyChanged, IAfmPlatform
    {
        private double _currentPositionX;
        private double _currentPositionY;
        private double _currentPositionZ;
        private bool _isHomeComplete;

        public IAxis XAxis { get; }
        public IAxis YAxis { get; }
        public IAxis ZAxis { get; }

        public AfmPlatformConfig Config { get; set; }

        public bool IsConnected => true;

        public double CurrentPositionX
        {
            get => _currentPositionX;
            set => SetProperty(ref _currentPositionX, value);
        }
        public double CurrentPositionY
        {
            get => _currentPositionY;
            set => SetProperty(ref _currentPositionY, value);
        }
        public double CurrentPositionZ
        {
            get => _currentPositionZ;
            set => SetProperty(ref _currentPositionZ, value);
        }

        public bool IsHomeComplete => _isHomeComplete;

        public string Name => throw new NotImplementedException();

        public AfmPlatform(AfmPlatformConfig config, IAxis[] axes)
        {
            XAxis = axes[0];
            YAxis = axes[1];
            ZAxis = axes[2];
            Config = config;
        }

        private void LoadAxesVelocity()
        {
            XAxis.LoadVelocity(Config.XWorkVel);
            YAxis.LoadVelocity(Config.YWorkVel);
        }

        /// <summary>
        /// 压印部分回零完成后，AFM Z轴回零
        /// 回零顺序：点胶Z回零》AFM Z》AFM X和Y
        /// </summary>
        /// <returns></returns>
        public bool GoHome()
        {
            _isHomeComplete = false;
            var zMoveMent = Task.Run(() => ZAxis.GoHome());
            Task.WaitAll(zMoveMent);
            var xMovement = Task.Run(() => XAxis.GoHome());
            var yMovement = Task.Run(() => YAxis.GoHome());        
            Task.WaitAll(xMovement, yMovement);
            _isHomeComplete = true;
            return true;
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
        public void Connect()
        {
        }

        public void Disconnect()
        {
        }
       
    }


    public class AfmPlatformConfig : NotifyPropertyChanged
    {
        private double _xWorkVel;
        private double _yWorkVel;
        private double _foundDistance;//蠕动寻找Mark的距离

        private Point2D _waitPosition = new Point2D(0, 0);
        private Point2D _workPosition = new Point2D(0, 0);


        [Category("AfmPlatform"), Description("X轴运行速度")]
        [DisplayName("X轴运行速度")]
        public double XWorkVel
        {
            get => _xWorkVel;
            set => SetProperty(ref _xWorkVel, value);
        }
        

        [Category("AfmPlatform"), Description("Y轴运行速度")]
        [DisplayName("Y轴运行速度")]
        public double YWorkVel
        {
            get => _yWorkVel;
            set => SetProperty(ref _yWorkVel, value);
        }

        [Category("AfmPlatform"), Description("等待位置")]
        [DisplayName("等待位置")]
        public Point2D WaitPosition
        {
            get => _waitPosition;
            set => SetProperty(ref _waitPosition, value);
        }

        [Category("AfmPlatform"), Description("工作位置")]
        [DisplayName("工作位置")]
        public Point2D WorkPosition
        {
            get => _workPosition;
            set => SetProperty(ref _workPosition, value);
        }

        [Category("AfmPlatform"), Description("工作位置")]
        [DisplayName("工作位置")]
        public double FoundDistance
        {
            get => _foundDistance;
            set => SetProperty(ref _foundDistance, value);
        }
    }
}
