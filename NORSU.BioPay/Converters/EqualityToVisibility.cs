using System;
using System.Globalization;
using System.Windows;

namespace NORSU.BioPay.Converters
{
    class EqualityConverter : ConverterBase
    {

        public enum ReturnTypes
        {
            Visibility, Boolean, Long, Object
        }

        public enum Operations
        {
            Equals,
            GreaterThan,
            LessThan,
            NotEquals,
        }

        private ReturnTypes _returnType = ReturnTypes.Visibility;

        public ReturnTypes ReturnType
        {
            get => _returnType;
            set
            {
                _returnType = value;
                if (value == ReturnTypes.Boolean)
                {
                    TrueValue = true;
                    FalseValue = false;
                }
            }
        }

        public object TrueValue { get; set; } = Visibility.Visible;

        public object FalseValue { get; set; } = Visibility.Collapsed;

        public Operations Operation { get; set; } = Operations.Equals;
        public object Operand { get; set; } = 0;

        public EqualityConverter(object whenTrue, object whenFalse)
        {
            TrueValue = whenTrue;
            FalseValue = whenFalse;
        }

        public EqualityConverter()
        {

        }

        protected override object Convert(object value, Type targetType, object parameter)
        {
            // if (targetType != trueVisibility.GetType()) return Binding.DoNothing;

            if (value == null)
                return FalseValue;

            if (parameter != null)
                return value.Equals(parameter) ? TrueValue : FalseValue;

            if (Operation == Operations.GreaterThan)
            {
                return double.Parse(value.ToString()) > System.Convert.ToDouble(Operand) ? TrueValue : FalseValue;
            }
            if (Operation == Operations.LessThan)
            {
                return double.Parse(value.ToString()) < System.Convert.ToDouble(Operand) ? TrueValue : FalseValue;
            }
            if (Operation == Operations.NotEquals)
                return value.Equals(Operand) ? FalseValue : TrueValue;
            return value.Equals(Operand) ? TrueValue : FalseValue;
        }
        
    }
}
