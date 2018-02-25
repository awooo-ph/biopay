using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace NORSU.BioPay.Converters
{
    class IsNullConverter:ConverterBase
    {
        public bool ReturnBoolean { get; set; }
        public bool Invert { get; set; }
        
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (ReturnBoolean)
                if (Invert)
                    return value != null;
                else
                    return value == null;
            
            if (value == null)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
    }
}
