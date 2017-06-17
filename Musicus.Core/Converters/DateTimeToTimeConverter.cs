using System;
using Windows.UI.Xaml.Data;

namespace Musicus.Core.Converters
{
    public class DateTimeToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
           return ((DateTime)value).Date;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
