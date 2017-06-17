using System;
using Windows.UI.Xaml.Data;

namespace Musicus.Core.Converters
{
   public class FirstLetterPlaylistImageTextCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return FirstCharToUpper((string)value);
        }

        public static string FirstCharToUpper(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            char[] array = value.ToCharArray();
            int count = 1; string obj = "";
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                obj = "" + char.ToUpper(array[0]);
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    count += 1;
                    obj = obj + char.ToUpper(array[i]);
                    if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    {
                        if (count >= 2) break;
                    }
                    else if (count >= 2) break;

                }
            }
            return obj.Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
