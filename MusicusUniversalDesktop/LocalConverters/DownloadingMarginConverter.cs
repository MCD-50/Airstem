using System;
using Windows.UI.Xaml.Data;

namespace Musicus
{
    public class DownloadingMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var state = (Data.Collection.Model.SongState)value;
            if (state == Data.Collection.Model.SongState.Downloading)
                return new Windows.UI.Xaml.Thickness(10, 0, 10, 0);
            return new Windows.UI.Xaml.Thickness(0, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
