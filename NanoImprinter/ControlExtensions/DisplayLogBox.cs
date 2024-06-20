using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NanoImprinter.ControlExtensions
{
    public class DisplayLogBox : RichTextBox
    {
        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(
            "TextColor",
            typeof(Color),
            typeof(DisplayLogBox),
            new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.None));

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(DisplayLogBox),
            new PropertyMetadata(null, new PropertyChangedCallback(OnTextPropertyChanged)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DisplayLogBox display = sender as DisplayLogBox;

            if (display != null)
            {
                string text = e.NewValue as string;
                char[] enterCharAry = ("\r\n").ToCharArray();
                text = text.TrimStart(enterCharAry);
                text = text.TrimEnd(enterCharAry);
                Run run = new Run(text);
                Paragraph paragraph = new Paragraph();
                paragraph.Foreground = new SolidColorBrush(display.TextColor);
                paragraph.Inlines.Add(run);
                display.Document.Blocks.Add(paragraph);
                display.ScrollToEnd();
            }
        }
    }

}
