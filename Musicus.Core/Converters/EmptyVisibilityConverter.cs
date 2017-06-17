
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
namespace Musicus.Core.Converters
{
    public class EmptyVisibilityConverter : IValueConverter
    {
        public bool Reverse { get; set; }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!Reverse)
            {
                if (string.IsNullOrEmpty(((string)value)))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
            else
            {
                if (string.IsNullOrEmpty(((string)value)))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
