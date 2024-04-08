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
    /// WorkFlowView.xaml 的交互逻辑
    /// </summary>
    public partial class WorkFlowView : UserControl
    {
        public static readonly DependencyProperty LoadColorProperty = DependencyProperty.Register("LoadColor",
        typeof(Brush), typeof(WorkFlowView), new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty GlueColorProperty = DependencyProperty.Register("GlueColor",
        typeof(Brush), typeof(WorkFlowView), new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty PreprintColorProperty = DependencyProperty.Register("PreprintColor",
        typeof(Brush), typeof(WorkFlowView), new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty ImprintColorProperty = DependencyProperty.Register("ImprintColor",
        typeof(Brush), typeof(WorkFlowView), new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty UVColorProperty = DependencyProperty.Register("UVColor",
        typeof(Brush), typeof(WorkFlowView), new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty DemoldColorProperty = DependencyProperty.Register("DemoldColor",
        typeof(Brush), typeof(WorkFlowView), new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty PositionColorProperty = DependencyProperty.Register("PositionColor",
        typeof(Brush), typeof(WorkFlowView), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty MaskLifetimeCountProperty = DependencyProperty.Register("MaskLifetimeCount",
        typeof(int), typeof(WorkFlowView), new PropertyMetadata(100));

        public static readonly DependencyProperty MaskUsageCountProperty = DependencyProperty.Register("MaskUsageCount",
        typeof(int), typeof(WorkFlowView), new PropertyMetadata(10));

        public WorkFlowView()
        {
            InitializeComponent();
        }
        public Brush DemoldColor
        {
            get => (Brush)GetValue(DemoldColorProperty);
            set =>
                SetValue(DemoldColorProperty, value);
        }

        public Brush GlueColor
        {
            get => (Brush)GetValue(GlueColorProperty);
            set => SetValue(GlueColorProperty, value);
            
        }

        public Brush ImprintColor
        {
            get => (Brush)GetValue(ImprintColorProperty);
            set => SetValue(ImprintColorProperty, value);
        }

        public Brush LoadColor
        {
            get => (Brush)GetValue(LoadColorProperty);
            set => SetValue(LoadColorProperty, value);
        }

        public Brush PositionColor
        {
            get => (Brush)GetValue(PositionColorProperty);
            set => SetValue(PositionColorProperty, value);
        }

        public Brush PreprintColor
        {
            get => (Brush)GetValue(PreprintColorProperty);
            set => SetValue(PreprintColorProperty, value);
        }
        public Brush UVColor
        {
            get => (Brush)GetValue(UVColorProperty);
            set => SetValue(UVColorProperty, value);
           
        }

        public int MaskLifetimeCount
        {
            get => (int)GetValue(MaskLifetimeCountProperty);
            set
            {
                SetValue(MaskLifetimeCountProperty, value);
            }
        }

        public int MaskUsageCount
        {
            get => (int)GetValue(MaskUsageCountProperty);
            set
            {
                SetValue(MaskUsageCountProperty, value);
            }
        }
    }
}
