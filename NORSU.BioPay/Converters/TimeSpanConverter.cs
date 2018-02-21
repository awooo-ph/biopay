using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NORSU.BioPay.Converters
{
    class TimeSpanConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var span = value as TimeSpan?;
            if (span == null) return "N/A";
            return span.Value.Hours.ToString("00") + ":" + span.Value.Minutes.ToString("00");
        }
    }
}
