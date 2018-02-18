using System;
using System.Windows;

namespace NORSU.BioPay.Converters {
    class BooleanToVisibility : ConverterBase
    {
        private Visibility trueVisibility = Visibility.Visible;
        private Visibility falseVisibility = Visibility.Collapsed;

        public BooleanToVisibility(Visibility whenTrue,Visibility whenFalse)
        {
            trueVisibility = whenTrue;
            falseVisibility = whenFalse;
        }
        public BooleanToVisibility() { }

        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (parameter != null) trueVisibility = (Visibility) parameter;
            if(value==null) return falseVisibility;
            if(value is bool)
                return (bool) value ? trueVisibility : falseVisibility;
            return trueVisibility;
        }
    }
}
