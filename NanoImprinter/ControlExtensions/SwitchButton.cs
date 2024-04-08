using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace NanoImprinter.ControlExtensions
{
    public class SwitchButton : ToggleButton
    {

        public static readonly DependencyProperty TextAProperty =
          DependencyProperty.Register("TextA", typeof(string), typeof(SwitchButton), new PropertyMetadata("ON"));

        public static readonly DependencyProperty TextBProperty =
          DependencyProperty.Register("TextB", typeof(string), typeof(SwitchButton), new PropertyMetadata("OFF"));

        public string TextA
        {
            get { return (string)GetValue(TextAProperty); }
            set { SetValue(TextAProperty, value); }
        }

        public string TextB
        {
            get { return (string)GetValue(TextBProperty); }
            set { SetValue(TextBProperty, value); }
        }


        public static readonly DependencyProperty TextABackgroundProperty =
          DependencyProperty.Register("TextABackground", typeof(Brush), typeof(SwitchButton), new PropertyMetadata(Brushes.Green));

        public static readonly DependencyProperty TextBBackgroundProperty =
          DependencyProperty.Register("TextBBackground", typeof(Brush), typeof(SwitchButton), new PropertyMetadata(Brushes.Red));

        public Brush TextABackground
        {
            get { return (Brush)GetValue(TextABackgroundProperty); }
            set { SetValue(TextABackgroundProperty, value); }
        }

        public Brush TextBBackground
        {
            get { return (Brush)GetValue(TextBBackgroundProperty); }
            set { SetValue(TextBBackgroundProperty, value); }
        }

    }
}
