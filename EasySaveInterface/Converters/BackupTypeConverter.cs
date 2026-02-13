using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.Model;

namespace EasySaveInterface.Converters
{
    public class BackupTypeConverter : IValueConverter
    {
        public static string FullText = "Full";
        public static string DifferentialText = "Differential";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BackupType type)
                return type == BackupType.Full ? FullText : DifferentialText;
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
