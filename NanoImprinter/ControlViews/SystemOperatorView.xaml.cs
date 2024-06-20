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
    /// SystemOperatorView.xaml 的交互逻辑
    /// </summary>
    public partial class SystemOperatorView : UserControl
    {
        public static readonly DependencyProperty StartCommandProperty = DependencyProperty.Register("StartCommand",
        typeof(ICommand), typeof(SystemOperatorView), new PropertyMetadata(null));
        public static readonly DependencyProperty EmergencyCommandProperty = DependencyProperty.Register("EmergencyCommand",
        typeof(ICommand), typeof(SystemOperatorView), new PropertyMetadata(null));
        public static readonly DependencyProperty ResetCommandProperty = DependencyProperty.Register("ResetCommand",
        typeof(ICommand), typeof(SystemOperatorView), new PropertyMetadata(null));
        public static readonly DependencyProperty GoHomeCommandProperty = DependencyProperty.Register("GoHomeCommand",
        typeof(ICommand), typeof(SystemOperatorView), new PropertyMetadata(null));

        /// <summary>
        /// 真空
        /// </summary>
        public static readonly DependencyProperty VacuumCommandProperty = DependencyProperty.Register("VacuumCommand",
        typeof(ICommand), typeof(SystemOperatorView), new PropertyMetadata(null));
        /// <summary>
        /// 破真空
        /// </summary>
        public static readonly DependencyProperty EvacuateCommandProperty = DependencyProperty.Register("EvacuateCommand",
        typeof(ICommand), typeof(SystemOperatorView), new PropertyMetadata(null));

      
        public ICommand StartCommand
        {
            get => (ICommand)GetValue(StartCommandProperty);
            set => SetValue(StartCommandProperty, value);
        }
        public ICommand EmergencyCommand
        {
            get => (ICommand)GetValue(EmergencyCommandProperty);
            set => SetValue(EmergencyCommandProperty, value);
        }
        public ICommand ResetCommand
        {
            get => (ICommand)GetValue(ResetCommandProperty);
            set => SetValue(ResetCommandProperty, value);
        }
        public ICommand GoHomeCommand
        {
            get => (ICommand)GetValue(GoHomeCommandProperty);
            set => SetValue(GoHomeCommandProperty, value);
        }
        public ICommand VacuumCommand
        {
            get => (ICommand)GetValue(VacuumCommandProperty);
            set => SetValue(VacuumCommandProperty, value);
        }
        public ICommand EvacuateCommand
        {
            get => (ICommand)GetValue(EvacuateCommandProperty);
            set => SetValue(EvacuateCommandProperty, value);
        }

        /// <summary>
        /// 手动或自动
        /// </summary>
        public static readonly DependencyProperty IsAutoProperty = DependencyProperty.Register("IsAuto",
        typeof(bool), typeof(SystemOperatorView), new PropertyMetadata(false));
        public static readonly DependencyProperty StartContentProperty = DependencyProperty.Register("StartContent",
        typeof(string), typeof(SystemOperatorView), new PropertyMetadata("启动"));
        public bool IsAuto
        {
            get => (bool)GetValue(IsAutoProperty);
            set => SetValue(IsAutoProperty, value);
        }
        public string StartContent
        {
            get => (string)GetValue(StartContentProperty);
            set => SetValue(StartContentProperty, value);
        }
        public SystemOperatorView()
        {
            InitializeComponent();
        }
    }
}
