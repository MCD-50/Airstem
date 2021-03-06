﻿using System;
using System.IO;

namespace Musicus.Core.Common
{
    public interface IBitmapImage
    {
        Uri Uri { get; }
        object Image { get; }
        void SetUri(Uri uri);
        void SetStream(Stream stream);
        void SetDecodedPixel(int size);
    }
}