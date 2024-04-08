using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NanoImprinter.ControlViews
{
    /// <summary>
    /// VisionView.xaml 的交互逻辑
    /// </summary>
    public partial class VisionView : UserControl
    {
        public static readonly DependencyProperty ShutterValueProperty = DependencyProperty.Register("ShutterValue",
        typeof(int), typeof(PrintMaskDataView), new PropertyMetadata(null));
        public static readonly DependencyProperty GainValueProperty = DependencyProperty.Register("GainValue",
       typeof(int), typeof(PrintMaskDataView), new PropertyMetadata(null));

        public VisionView()
        {
            InitializeComponent();
        }
        public int ShutterValue
        {
            get => (int)GetValue(ShutterValueProperty);
            set => SetValue(ShutterValueProperty, value);
        }

        public int GainValue
        {
            get => (int)GetValue(GainValueProperty);
            set => SetValue(GainValueProperty, value);
        }
    }
}
