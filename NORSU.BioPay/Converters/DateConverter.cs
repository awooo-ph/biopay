using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NORSU.BioPay.Converters
{
    class DateConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (!(value is DateTime date) || date == DateTime.MinValue)
                return "N/A";
            return date.ToString("MMM d, yyyy");
        }
    }
}
