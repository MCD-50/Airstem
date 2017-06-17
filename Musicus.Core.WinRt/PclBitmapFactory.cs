#region

using System;
using Musicus.Core.Common;
using Musicus.Core.Utils.Interfaces;

#endregion

namespace Musicus.Core.WinRt
{
    public class PclBitmapFactory : IBitmapFactory
    {
        public IBitmapImage CreateImage(Uri uri)
        {
            return new PclBitmapImage(uri);
        }
    }
}