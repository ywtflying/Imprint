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
    public class IconButton : RadioButton
    {
        public static readonly DependencyProperty IconImageProperty = DependencyProperty.Register(
          "IconImage",
          typeof(string),
          typeof(IconButton),
          new PropertyMetadata());

        public static readonly DependencyProperty PathDataProperty = DependencyProperty.Register(
          "PathData",
           typeof(Geometry),
           typeof(IconButton),
           new PropertyMetadata());

        public static readonly DependencyProperty PathFillProperty = DependencyProperty.Register(
           "PathFill",
           typeof(SolidColorBrush),
           typeof(IconButton),
           new PropertyMetadata());

        public static readonly DependencyProperty PathStrokeProperty = DependencyProperty.Register(
          "PathStroke",
          typeof(SolidColorBrush),
          typeof(IconButton),
          new PropertyMetadata());

        public static readonly DependencyProperty TextContentProperty = DependencyProperty.Register(
           "TextContent",
           typeof(string),
           typeof(IconButton),
           new PropertyMetadata());

        public string IconImage
        {
            get { return (string)GetValue(IconImageProperty); }
            set { SetValue(IconImageProperty, value); }
        }

        /// <summary>
        /// 按钮字体图标编码
        /// </summary>
        public Geometry PathData
        {
            get { return (Geometry)GetValue(PathDataProperty); }
            set { SetValue(PathDataProperty, value); }
        }

        public SolidColorBrush PathFill
        {
            get { return (SolidColorBrush)GetValue(PathFillProperty); }
            set { SetValue(PathFillProperty, value); }
        }

        public SolidColorBrush PathStroke
        {
            get { return (SolidColorBrush)GetValue(PathStrokeProperty); }
            set { SetValue(PathStrokeProperty, value); }
        }

        public string TextContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }
    }
}
