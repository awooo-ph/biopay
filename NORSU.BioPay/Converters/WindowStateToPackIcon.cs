using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace NORSU.BioPay.Converters
{
    class WindowStateToPackIcon : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            var state = value as WindowState?;
            if (state == null) return Binding.DoNothing;
            var icon = new PackIcon();
            if (state.Value == WindowState.Maximized)
                icon.Kind = PackIconKind.WindowRestore;
            else icon.Kind = PackIconKind.WindowMaximize;
            return icon;
        }
    }
}
