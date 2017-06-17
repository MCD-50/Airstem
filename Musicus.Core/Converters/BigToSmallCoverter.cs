using System;
using Windows.UI.Xaml.Data;

namespace Musicus.Core.Converters
{
    public class BigToSmallCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return (Math.Floor((double)value*.7));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
