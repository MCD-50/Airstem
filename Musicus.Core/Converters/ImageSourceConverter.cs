﻿using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Musicus.Core.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
          //  int size;
          //  int.TryParse(parameter as string, out size);

            var url = value as string;
            Uri uri;
            if (url == null)
                uri = value as Uri;
            else
                uri = new Uri(url);

            if (uri == null)
                return null;

            var bitmap = new BitmapImage(uri);

           // if (size != 0)
           // {
           //     bitmap.DecodePixelHeight = size;
            //    bitmap.DecodePixelWidth = size;
            //}

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}