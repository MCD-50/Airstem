﻿#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Musicus.Core.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}