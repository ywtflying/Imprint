using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NanoImprinter.ControlExtensions
{
    public class IconTabItem : TabItem
    {
        public static readonly DependencyProperty HeaderContentProperty = DependencyProperty.Register(
          "HeaderContent",
          typeof(string),
          typeof(IconTabItem),
          new PropertyMetadata());

        public static readonly DependencyProperty PathDataProperty = DependencyProperty.Register(
          "PathData",
           typeof(Geometry),
           typeof(IconTabItem),
           new PropertyMetadata());


        public Geometry PathData
        {
            get { return (Geometry)GetValue(PathDataProperty); }
            set { SetValue(PathDataProperty, value); }
        }

        public string HeaderContent
        {
            get { return (string)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }
    }

}
