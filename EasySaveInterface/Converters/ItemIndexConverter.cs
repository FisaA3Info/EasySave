using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using Avalonia.Data.Converters;

namespace EasySaveInterface.Converters
{
    public class ItemIndexConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Count < 2)
                return string.Empty;

            var collection = values[0] as IEnumerable;
            var item = values[1];

            if (collection == null || item == null)
                return string.Empty;

            int index = 0;
            foreach (var x in collection)
            {
                if (ReferenceEquals(x, item) || (x != null && x.Equals(item)))
                    return (index + 1).ToString();
                index++;
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
