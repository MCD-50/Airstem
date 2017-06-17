#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Musicus.Core.Converters
{
    public class StringToVisibiltyConverter : IValueConverter
    {
        public StringToVisibiltyConverter()
        {
            Reverse = false;
        }

        public bool Reverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if(Reverse)
                return string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed;

            return string.IsNullOrEmpty((string) value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}