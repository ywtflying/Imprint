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
using WestLakeShape.Motion.Device;

namespace NanoImprinter.Model
{
    public interface IMacroPlatform : INotifyPropertyChanged, IPlatform
    {
        MacroPlatformConfig Config { get; set; }
        bool MoveTo(PointXYR offsetValue);
        bool MoveTo(Point2D offsetValue);
        //bool GoHome();
        bool MoveToLoadPosition();
        bool MoveToImprintPosition();
        bool MoveToGluePosition();
        void ResetAxesAlarm();
    }

    public class MacroPlatform: IMacroPlatform
    {
        private double _currentPositionX;
        private double _currentPositionY;
        private double _currentPositionR;
        private bool _isHomeComplete;
        public IAxis XAxis { get; }
        public IAxis X2Axis { get; }
        public IAxis YAxis { get; }
        public IAxis RAxis { get; }
        public bool IsConnected => true;

        public MacroPlatformConfig Config
        {
            get; set;
        }

        public double CurrentPositionX
        {
            get => _currentPositionX;
            set 
            {
                if (_currentPositionX != value)
                {
                    var val = Math.Round(value, 6);
                    _currentPositionX = val;
                    OnPropertyChanged(nameof(CurrentPositionX));
                }
            }
        }
        public double CurrentPositionY
        {
            get => _currentPositionY;
            set
            {
                if (_currentPositionY != value)
                {
                    var val = Math.Round(value, 6);
                    _currentPositionY = val;
                    OnPropertyChanged(nameof(CurrentPositionY));
                }
            }
        }
        public double CurrentPositionR
        {
            get => _currentPositionR;
            set
            {
                if (_currentPositionR != value)
                {
                    var val = Math.Round(value, 6);
                    _currentPositionR = val;
                    OnPropertyChanged(nameof(CurrentPositionR));
                }
            }
        }

        public bool IsHomeComplete => _isHomeComplete;

        public string Name => throw new NotImplementedException();

        public MacroPlatform(MacroPlatformConfig config,IAxis[] axes)
        {
            Config = config;
            XAxis = axes[0];
            X2Axis = axes[1];
            YAxis = axes[2];
            RAxis = axes[3];
            _isHomeComplete = true;
            RefreshDataService.Instance.Register(RefreshRealtimeData);
        }

        public List<IAxis> Axes()
        {
            return new List<IAxis>
            {
                XAxis,
                YAxis,
                RAxis
            };
        }
        private void LoadAxesVelocity()
        {
            XAxis.LoadVelocity(Config.XWorkVel);
            YAxis.LoadVelocity(Config.YWorkVel);
            RAxis.LoadVelocity(Config.RWorkVel);
        }

        /// <summary>
        /// XY方向和R方向的移动
        /// </summary>
        /// <param name="offsetValue"></param>
        /// <returns></returns>
        public bool MoveTo(PointXYR offsetValue)
        {
            return MoveBy(offsetValue.X,
                          offsetValue.Y,
                          offsetValue.R);
           
        }

        /// <summary>
        /// XY方向的移动
        /// </summary>
        /// <param name="offsetValue"></param>
        /// <returns></returns>
        public bool MoveTo(Point2D offsetValue)
        {
            return MoveBy(offsetValue.X,
                          offsetValue.Y,
                          0);
        }

        /// <summary>
        /// 微动平台回零后，宏动平台回零
        /// </summary>
        /// <returns></returns>
        public bool GoHome()
        {
            _isHomeComplete = false;
            var xMovement = Task.Run(()=> XAxis.GoHome());
            var yMovement = Task.Run(() => YAxis.GoHome());
            var rMovement = Task.Run(() => RAxis.GoHome());
           
            Task.WaitAll(xMovement, yMovement, rMovement);
            
            _isHomeComplete = true;
            return _isHomeComplete;
        }

        public bool MoveToLoadPosition()
        {
            return MoveBy(Config.LoadPosition.X,
                          Config.LoadPosition.Y,
                          0);
        }
        public bool MoveToImprintPosition()
        {
            return MoveBy(Config.ImprintPosition.X,
                          Config.ImprintPosition.Y,
                          0);
        }

        public bool MoveToGluePosition()
        {
            return MoveBy(Config.GluePosition.X,
                          Config.GluePosition.Y,
                          0);
        }

        public void ResetAxesAlarm()
        {
            ((TrioAxis)XAxis).ResetAlarm();
            //((TrioAxis)YAxis).ResetAlarm();
            ((TrioAxis)RAxis).ResetAlarm();
        }

        private bool MoveBy(double x, double y, double r)
        {
            var xMovement = Task.Run(() => XAxis.MoveTo(x));
            //var yMovement = Task.Run(() => YAxis.MoveTo(y));
            //var rMovement = Task.Run(() => RAxis.MoveTo(r));
            //Task.WaitAll(xMovement, yMovement, rMovement);
            //Task.WaitAll(xMovement, rMovement);
            return true;
        }
        private void RefreshRealtimeData()
        {
            if (_currentPositionX != XAxis.Position)
                CurrentPositionX = XAxis.Position;
            if (_currentPositionY != YAxis.Position)
                CurrentPositionY = YAxis.Position;
            if (_currentPositionR != RAxis.Position)
                CurrentPositionR = RAxis.Position;
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

        public void Connect()
        {
        }
        public void Disconnect()
        {
        }
    }


    public class MacroPlatformConfig: NotifyPropertyChanged
    {
        private Point2D _loadPosition = new Point2D(0, 0);
        private Point2D _gluePosition = new Point2D(0, 0);
        private Point2D _imprintPosition = new Point2D(0, 0);
        private Point2D _leftCenterPosition = new Point2D(0, 0);
        private Point2D _rightCenterPosition = new Point2D(0, 0);
        private Point2D _upCenterPosition = new Point2D(0, 0);
        private Point2D _downCenterPosition = new Point2D(0, 0);
        private double _xWorkVel;
        private double _yWorkVel;
        private double _rWorkVel;

        public string Name => "宏动平台";
        public double XWorkVel
        {
            get => _xWorkVel;
            set => SetProperty(ref _xWorkVel, value);
        }
        public double YWorkVel
        {
            get => _yWorkVel;
            set => SetProperty(ref _yWorkVel, value);
        }
        public double RWorkVel
        {
            get => _rWorkVel;
            set => SetProperty(ref _rWorkVel, value);
        }

        [Category("MacroPlatform"), Description("放晶圆位置")]
        [DisplayName("放料位置")]
        public Point2D LoadPosition
        {
            get => _loadPosition;
            set => SetProperty(ref _loadPosition, value);
        }

        [Category("MacroPlatform"), Description("点胶位置")]
        [DisplayName("点胶位置")]
        public Point2D GluePosition 
        {
            get => _gluePosition;
            set => SetProperty(ref _gluePosition, value);
        }


        [Category("MacroPlatform"), Description("压印位置")]
        [DisplayName("压印位置")]
        public Point2D ImprintPosition 
        {
            get => _imprintPosition ;
            set => SetProperty(ref _imprintPosition, value);
        }

        [Category("MacroPlatform"), Description("圆心左监测点")]
        [DisplayName("圆心左监测点")]
        public Point2D LeftCenterPosition 
        {
            get => _leftCenterPosition ;
            set => SetProperty(ref _leftCenterPosition, value);
        }

        [Category("MacroPlatform"), Description("圆心右监测点")]
        [DisplayName("圆心右监测点")]
        public Point2D RightCenterPosition
        {
            get => _rightCenterPosition;
            set => SetProperty(ref _rightCenterPosition, value);
        }


        [Category("MacroPlatform"), Description("圆心上监测点")]
        [DisplayName("圆心上监测点")]
        public Point2D UpCenterPosition
        {
            get => _upCenterPosition;
            set => SetProperty(ref _upCenterPosition, value);
        }

        [Category("MacroPlatform"), Description("圆心下监测点")]
        [DisplayName("圆心下监测点")]
        public Point2D DownCenterPosition
        {
            get => _downCenterPosition;
            set => SetProperty(ref _downCenterPosition, value);
        }


        //public TrioAxisConfig XAxisConfig { get; set; }
        //public TrioAxisConfig YAxisConfig { get; set; }
        //public TrioAxisConfig RAxisConfig { get; set; }
    }
}
