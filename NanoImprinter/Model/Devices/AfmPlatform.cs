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

        public void MoveToWaitPosition()
        {
            var xTask = Task.Run(() => MoveBy(XAxis, Config.WaitPosition.X));
            var yTask = Task.Run(() => MoveBy(YAxis, Config.WaitPosition.Y));
            var zTask = Task.Run(() => MoveBy(ZAxis, Config.WaitPosition.Z));
            Task.WaitAll(xTask, yTask, zTask);
        }
        public void MoveToWorkPosition()
        {
            var xTask = Task.Run(() => MoveBy(XAxis, Config.WorkPosition.X));
            var yTask = Task.Run(() => MoveBy(YAxis, Config.WorkPosition.Y));
            var zTask = Task.Run(() => MoveBy(ZAxis, Config.WorkPosition.Z));
            Task.WaitAll(xTask, yTask, zTask);
        }
        private bool MoveBy(IAxis axis, double position)
        {
            return axis.MoveTo(position);
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
        private double _zWorkVel;
        private double _foundDistance;//蠕动寻找Mark的距离

        private Point3D _waitPosition = new Point3D(0, 0,0);
        private Point3D _workPosition = new Point3D(0, 0,0);


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
        [Category("AfmPlatform"), Description("Y轴运行速度")]
        [DisplayName("Y轴运行速度")]
        public double ZWorkVel
        {
            get => _zWorkVel;
            set => SetProperty(ref _zWorkVel, value);
        }

        [Category("AfmPlatform"), Description("等待位置")]
        [DisplayName("等待位置")]
        public Point3D WaitPosition
        {
            get => _waitPosition;
            set => SetProperty(ref _waitPosition, value);
        }

        [Category("AfmPlatform"), Description("工作位置")]
        [DisplayName("工作位置")]
        public Point3D WorkPosition
        {
            get => _workPosition;
            set => SetProperty(ref _workPosition, value);
        }


    }
}
