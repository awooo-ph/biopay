using System;
using System.Globalization;

namespace NORSU.BioPay.Converters {
    class Inverter : ConverterBase {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return ! (bool) value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
