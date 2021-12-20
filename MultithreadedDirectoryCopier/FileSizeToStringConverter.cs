using System;
using System.Globalization;
using System.Windows.Data;


namespace MultithreadedDirectoryCopier {
    class FileSizeToStringConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is null)
                return null;
            if (!(value is long condition))
                throw new ArgumentException($"Исходное значение должно иметь тип {nameof(condition)}");
            if (targetType != typeof(string))
                throw new InvalidCastException();
            return $"{(long)value:N0}";
        }


        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

    }
}
