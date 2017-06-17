#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Musicus.Data.Collection.Model;

#endregion

namespace Musicus
{
    public class SongStateConverter : IValueConverter
    {
        public SongState SongState { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (SongState) value;
            return state == SongState ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}