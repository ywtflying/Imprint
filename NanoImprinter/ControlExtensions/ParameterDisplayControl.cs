using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NanoImprinter.ControlExtensions
{
    public class ParameterDisplayControl: Control
    {
        public static readonly DependencyProperty ParamNameProperty = DependencyProperty.Register(
          "ParamName",
          typeof(String),
          typeof(ParameterDisplayControl));

        public static readonly DependencyProperty ParamValueProperty = DependencyProperty.Register(
          "ParamValue",
          typeof(string),
          typeof(ParameterDisplayControl),
          new PropertyMetadata());

        public static readonly DependencyProperty ParamUnitProperty = DependencyProperty.Register(
            "ParamUnit",
            typeof(String),
            typeof(ParameterDisplayControl));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
          "IsReadOnly",
          typeof(bool),
          typeof(ParameterDisplayControl),
          new PropertyMetadata());

        public String ParamName
        {
            get { return (String)GetValue(ParamNameProperty); }
            set { SetValue(ParamNameProperty, value); }
        }
        public string ParamValue
        {
            get { return (string)GetValue(ParamValueProperty); }
            set { SetValue(ParamValueProperty, value); }
        }
        public String ParamUnit
        {
            get { return (String)GetValue(ParamUnitProperty); }
            set { SetValue(ParamUnitProperty, value); }
        }
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
    }

}
