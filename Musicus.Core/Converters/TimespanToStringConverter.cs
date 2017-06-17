#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Musicus.Core.Converters
{
    public class TimespanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return ((TimeSpan)value).ToString("hh\\:mm\\:ss");
        }


        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}