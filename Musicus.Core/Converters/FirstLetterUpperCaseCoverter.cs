using System;
using Windows.UI.Xaml.Data;

namespace Musicus.Core.Converters
{
    public class FirstLetterUpperCaseCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
           string name = FirstCharToUpper((string)value);
           return name;
        }

        public static string FirstCharToUpper(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
