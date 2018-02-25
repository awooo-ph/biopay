using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models;

namespace NORSU.BioPay.Converters
{
    class IsScheduleConverter :ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return value is Schedule;
        }
    }
}
