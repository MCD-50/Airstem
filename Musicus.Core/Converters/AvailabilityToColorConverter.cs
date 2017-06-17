using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Musicus.Core.Converters
{
    public class AvailabilityToColorConverter : IValueConverter
    {
        public bool toogle = false;
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var Availability = (bool)value;
            if (!toogle)
            {
                toogle = true;
                return (new SolidColorBrush(Colors.Black));
            }
            else
            {
                toogle = false;
                return (new SolidColorBrush(Colors.DarkGray));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
