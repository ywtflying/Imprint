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
    /// PrintMaskDataView.xaml 的交互逻辑
    /// </summary>
    public partial class PrintMaskDataView : UserControl
    {
        public static readonly DependencyProperty ImprintColProperty = DependencyProperty.Register("ImprintCol",
        typeof(int), typeof(PrintMaskDataView), new PropertyMetadata(null));

        public static readonly DependencyProperty ImprintCountProperty = DependencyProperty.Register("ImprintCount",
        typeof(int), typeof(PrintMaskDataView), new PropertyMetadata(null));

        public static readonly DependencyProperty ImprintRowProperty = DependencyProperty.Register("ImprintRow",
        typeof(int), typeof(PrintMaskDataView), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentColProperty = DependencyProperty.Register("CurrentCol",
        typeof(int), typeof(PrintMaskDataView), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentIndexProperty = DependencyProperty.Register("CurrentIndex",
        typeof(int), typeof(PrintMaskDataView), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentRowProperty = DependencyProperty.Register("CurrentRow",
        typeof(int), typeof(PrintMaskDataView), new PropertyMetadata(null));
        public PrintMaskDataView()
        {
            InitializeComponent();
        }

        public int ImprintCol
        {
            get => (int)GetValue(ImprintColProperty);
            set => SetValue(ImprintColProperty, value);
        }

        public int ImprintCount
        {
            get => (int)GetValue(ImprintCountProperty);
            set => SetValue(ImprintCountProperty, value);

        }

        public int ImprintRow
        {
            get => (int)GetValue(ImprintRowProperty);
            set => SetValue(ImprintRowProperty, value);

        }

        public int CurrentCol
        {
            get => (int)GetValue(CurrentColProperty);
            set => SetValue(CurrentColProperty, value);
        }

        public int CurrentIndex
        {
            get => (int)GetValue(CurrentIndexProperty);
            set => SetValue(CurrentIndexProperty, value);

        }
        public int CurrentRow
        {
            get => (int)GetValue(CurrentRowProperty);
            set => SetValue(CurrentRowProperty, value);

        }
    }
}
