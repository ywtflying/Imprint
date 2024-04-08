using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NanoImprinter.Converters
{
    public class EnumToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (value is DeviceStatus yourEnumValue)
            //{
            //    var colorName = yourEnumValue.GetDescription();
            //    var color = (Color)ColorConverter.ConvertFromString(colorName);
            //    return new SolidColorBrush(color);
            //}
            //return Brushes.Transparent;
            if (value == null || !(value is Enum))
                return Brushes.Transparent;

            var enumValue = (Enum)value;
            var colorName = enumValue.GetDescription();
            var color = (Color)ColorConverter.ConvertFromString(colorName);
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
