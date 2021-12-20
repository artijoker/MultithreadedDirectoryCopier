using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MultithreadedDirectoryCopier {
    class BoolToVisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is null)
                return null;
            if (!(value is bool condition))
                throw new ArgumentException($"Исходное значение должно иметь тип {nameof(condition)}");
            if (targetType != typeof(Visibility))
                throw new InvalidCastException();
            if ((bool)value == true)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }


    }
}
