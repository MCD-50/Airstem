﻿#region

using System;
using System.IO;
using Windows.UI.Xaml.Media.Imaging;
using Musicus.Core.Common;
using System.Runtime.Serialization;

#endregion

namespace Musicus.Core.WinRt
{
    
    public class PclBitmapImage : IBitmapImage
    {
        private int _decoded;
        private Uri _uri;

        public PclBitmapImage(Uri uri)
        {
            _uri = uri;
        }

        public Uri Uri{get { return _uri; }}

        public object Image
        {
            get
            {
                return new BitmapImage(_uri)
                {
                    DecodePixelWidth = _decoded
                };
            }
        }

        public void SetUri(Uri uri)
        {
            _uri = uri;
        }

        public void SetStream(Stream stream)
        {
            // TODO
        }

        public void SetDecodedPixel(int size)
        {
            _decoded = size;
        }
    }
}