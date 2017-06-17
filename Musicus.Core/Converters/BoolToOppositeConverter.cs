#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Musicus.Core.Converters
{
    public class BoolToOppositeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return !(bool)value;
        }
    }
}