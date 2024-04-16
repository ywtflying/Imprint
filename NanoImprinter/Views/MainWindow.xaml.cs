using NanoImprinter.Model;
using Prism.Regions;
using System;
using System.Windows;
using System.Windows.Input;

namespace NanoImprinter.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDeviceManager _machine;
        public MainWindow(IRegionManager regionManager, IDeviceManager machine)
        {
            InitializeComponent();
            _machine = machine;
            regionManager.RegisterViewWithRegion("ContentRegion", typeof(MainView));

            this.btnMin.Click += (s, e) => { this.WindowState = WindowState.Minimized; };
            this.btnMax.Click += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            };
            this.btnClose.Click += (s, e) => { this.Close(); _machine.SaveParam(); };

            //this.gridTitle.MouseDoubleClick += (s, e)=>
            //{
            //    if (this.WindowState == WindowState.Normal)
            //        this.WindowState = WindowState.Maximized;
            //}
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }


        private void CurrentDomain_UnhandleException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
            {
                Exception ex = (Exception)e.ExceptionObject;
                string errCapiton = "Error!";
                MessageBox.Show(ex.Message, errCapiton, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
