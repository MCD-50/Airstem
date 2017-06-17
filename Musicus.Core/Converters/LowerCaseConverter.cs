using System;
using Windows.UI.Xaml.Data;

namespace Musicus.Core.Converters
{
    public class LowerCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {

            if (value == null)
                return "";
            return ((string)value).ToLower();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
