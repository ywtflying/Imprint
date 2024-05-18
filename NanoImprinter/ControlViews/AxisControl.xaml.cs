using NanoImprinter.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using WestLakeShape.Motion;

namespace NanoImprinter.ControlViews
{
    /// <summary>
    /// AxisControl.xaml 的交互逻辑
    /// </summary>
    public partial class AxisControl : UserControl
    {
        public AxisControl()
        {
            InitializeComponent();
            IsFixedSpeed = false;
        }

        public IAxis SelectedAxis {get; set;}

        public static readonly DependencyProperty IsFxiedSpeedProperty = DependencyProperty.Register("IsFixedSpeed",
             typeof(bool), typeof(AxisControl), new PropertyMetadata(true));

        public static readonly DependencyProperty AxesProperty = DependencyProperty.Register("Axes",
            typeof(IEnumerable<IAxis>), typeof(AxisControl), new PropertyMetadata(null));

        public static readonly DependencyProperty UnitNameProperty = DependencyProperty.Register("UnitName",
            typeof(string), typeof(AxisControl), new PropertyMetadata("mm/s"));
        
        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue",
           typeof(double), typeof(AxisControl), new PropertyMetadata(default(double)));


        /// <summary>
        /// 因为模版的原因导致IsChecked绑定失败，后期优化
        /// </summary>
        public bool IsFixedSpeed
        {
            get => (bool)GetValue(IsFxiedSpeedProperty);
            set
            {
                SetValue(IsFxiedSpeedProperty, value);
                UnitName = value? "mm/s" : "mm";
            }
        }
        public IEnumerable<IAxis> Axes
        {
            get => (IEnumerable<IAxis>)GetValue(AxesProperty);
            set => SetValue(AxesProperty, value);
              
        }
        public string UnitName
        {
            get => (string)GetValue(UnitNameProperty);
            set => SetValue(UnitNameProperty, value);
        }
        public double SelectedValue
        {
            get => (double)GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }
 

        private void btnJogForward_Click(object sender, RoutedEventArgs e)
        {    
            var axis = SelectedAxis;
            var dir = SelectedAxis.Direction ? 1 : -1;
            var distance = SelectedValue * dir;
            var task = Task.Run(()=> axis.MoveBy(distance));
        }
        private void btnJogBack_Click(object sender, RoutedEventArgs e)
        {
            var axis = SelectedAxis;
            //在移动方向上后退，值与运动方向值的符号相反
            var dir = SelectedAxis.Direction ? -1 : 1;
            var distance = dir * SelectedValue;
            var task = Task.Run(() => axis.MoveBy(distance));
        }

        private void SliderShowToolTip(object sender, RoutedEventArgs e)
        {
            double maximum = this.sldJogValue.Maximum;
            double minimum = this.sldJogValue.Minimum;
            double currentValue = this.sldJogValue.Value;
            // update the information to tool tip
            string toolTip = string.Format("{0:n} | min:{1:n}, max:{2:n}", currentValue, minimum, maximum);
            this.ToolTip = toolTip;
        }

        private void JogModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            UnitName = "mm";
            IsFixedSpeed = false;
        }

        private void JogModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UnitName = "mm/s";
            IsFixedSpeed = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            SelectedAxis.Stop();
        }

        private void btnGoHome_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SelectedAxis.GoHome()) ;        
        }
    }
}
