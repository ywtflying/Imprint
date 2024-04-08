using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WestLakeShape.Common
{
    public class Point : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Point2D:Point
    {
        private double _x;
        private double _y;
        public Point2D(double x =0, double y =0)
        {
            X = x;
            Y = y;
        }
        public double X 
        {
            get => _x;
            set 
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }
        public double Y 
        {
            get=>_y;
            set
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

      
    }

    public class Point3D : Point
    {
        private double _x;
        private double _y;
        private double _z;
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }
        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
        public double Z
        {
            get => _z;
            set
            {
                _z = value;
                OnPropertyChanged(nameof(Z));
            }
        }
    }

    public class PointXYR:Point
    {
        private double _x;
        private double _y;
        private double _r;
        public PointXYR(double x, double y, double r)
        {
            X = x;
            Y = y;
            R = r;
        }


        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }


        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }


        public double R
        {
            get => _r;
            set
            {
                _r = value;
                OnPropertyChanged(nameof(R));
            }
        }
    }

    public class PointXZ:Point
    {
        private double _x;
        private double _z;
        public PointXZ(double x, double z)
        {
            X = x;
            Z = z;
        }


        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }
        public double Z
        {
            get => _z;
            set
            {
                _z = value;
                OnPropertyChanged(nameof(Z));
            }
        }
    }

    /// <summary>
    /// Z轴平动，绕XY旋转
    /// </summary>
    public class PointZRXY:Point
    {
        private double _rx = 0;
        private double _ry = 0;
        private double _z = 0;
        public PointZRXY(double z, double rX, double rY)
        {
            Z = z;
            RX = rX;
            RY = rY;
        }
        public double Z
        {
            get => _z;
            set
            {
                _z = value;
                OnPropertyChanged(nameof(Z));
            }
        }
        public double RX
        {
            get => _rx;
            set
            {
                _rx = value;
                OnPropertyChanged(nameof(RX));
            }
        }
        public double RY
        {
            get => _ry;
            set
            {
                _ry = value;
                OnPropertyChanged(nameof(RY));
            }
        }
    }
}
