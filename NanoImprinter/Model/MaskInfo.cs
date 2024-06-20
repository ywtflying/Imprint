using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WestLakeShape.Common;
using WestLakeShape.Common.WpfCommon;

namespace NanoImprinter.Model
{
    public class MaskInfo : NotifyPropertyChanged
    {
        private int _imprintRow;
        private int _imprintCol;
        private int _impintCount;
        private int _currentIndex;
        private double _forceRangePercntage;
        private double _forceValue;
      

        private int _maskLifetimeCount;
        private int _maskUsageCount;
        private double _width;
        private double _length;

        private double _xOffse;
        private double _yOffse;

        /// <summary>
        /// 压印Mask的列数
        /// </summary>
        public int ImprintCol
        {
            get => _imprintCol;
            set => SetProperty(ref _imprintCol, value);
        }
        /// <summary>
        /// 压印Mask的行数
        /// </summary>
        public int ImprintRow
        {
            get => _imprintRow;
            set => SetProperty(ref _imprintRow, value);
        }

        /// <summary>
        /// 压印图案总数
        /// </summary>
        public int ImprintCount
        {
            get => _impintCount;
            set => SetProperty(ref _impintCount, value);
        }

        /// <summary>
        /// 当前正在压印的索引
        /// </summary>
        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _imprintCol, value);
        }
        /// <summary>
        /// 压力浮动范围，当达到该范围则停止压印
        /// </summary>
        public double ForceRangePercentage
        {
            get => _forceRangePercntage;
            set => SetProperty(ref _forceRangePercntage, value);
        }
        /// <summary>
        /// 设定压力值
        /// </summary>
        public double ForceValue
        {
            get => _forceValue;
            set => SetProperty(ref _forceValue, value);
        }

        /// <summary>
        /// Mask的长
        /// </summary>
        public double Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }
        /// <summary>
        /// Mask的宽
        /// </summary>
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }
        /// <summary>
        /// X方向间距
        /// </summary>
        public double XOffset
        {
            get => _xOffse;
            set => SetProperty(ref _xOffse, value);
        }
        /// <summary>
        /// Y方向间距
        /// </summary>
        public double YOffset
        {
            get => _yOffse;
            set => SetProperty(ref _yOffse, value);
        }

        /// <summary>
        /// 掩膜可压印次数
        /// </summary>
        public int MaskLifetimeCount
        {
            get => _maskLifetimeCount;
            set => SetProperty(ref _maskLifetimeCount, value);
        }

        /// <summary>
        /// 掩膜已压印次数
        /// </summary>
        public int MaskUsageCount
        {
            get => _maskUsageCount;
            set => SetProperty(ref _maskUsageCount, value);
        }

    }

    public class WafeInfo
    {
        public Point2D Center { get; set; }

        public Point2D Radius { get; set; }
    }
}
