using System;
using Musicus.Core.Common;

namespace Musicus.Core.Utils.Interfaces
{
    public interface IBitmapFactory
    {
        IBitmapImage CreateImage(Uri uri);
    }
}
