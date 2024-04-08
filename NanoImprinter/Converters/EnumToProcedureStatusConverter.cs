using NanoImprinter.Procedures;
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
    public class EnumToProcedureStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ProcedureStatus statusValue))
                return null;


            switch (statusValue)
            {
                case ProcedureStatus.Cancelled:
                    return Brushes.Red;
                case ProcedureStatus.Running:
                    return Brushes.Green;
                case ProcedureStatus.Stopped:
                    return Brushes.Orange;
                default:
                    return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
