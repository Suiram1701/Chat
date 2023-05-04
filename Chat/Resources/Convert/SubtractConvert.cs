using System;
using System.Globalization;
using System.Windows.Data;

namespace Chat.Resources.Convert
{
    [ValueConversion(typeof(double), typeof(double), ParameterType = typeof(double))]
    internal class SubtractConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            double sub = double.Parse((string)parameter);
            return val - sub;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
