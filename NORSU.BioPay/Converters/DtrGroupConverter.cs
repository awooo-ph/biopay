using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Models;

namespace NORSU.BioPay.Converters
{
    class DtrGroupConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (!(value is CollectionViewGroup group))
                return Binding.DoNothing;

            var mils = group.Items.Sum(x => (x as DailyTimeRecord)?.TimeSpan?.TotalMilliseconds) ?? 0;

            var span = TimeSpan.FromMilliseconds(mils);

            return $"{span.Hours:00} : {span.Minutes:00} : {span.Seconds:00}";
        }

        
    }
}
