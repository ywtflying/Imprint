using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoImprinter.ViewModels
{
    public class MaskDataViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<XYPoint> Points { get; set; } = new ObservableCollection<XYPoint>();

        public MaskDataViewModel()
        {
            // 这里可以添加初始化代码，或者加载初始点集
            // 循环添加点
            for (double i = 0; i < 100; i += 5)
            {
                var row = (int)(i / 10);
                var col = (int)(i % 10);
                AddPoint(row, col);
            }
        }

        public void AddPoint(int row, int col)
        {
            var point = new XYPoint()
            {
                X = row * 5 + 150,
                Y = col * 5 + 150
            };
            Points.Add(new XYPoint { X = point.X, Y = point.Y });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class XYPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
