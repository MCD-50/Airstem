using System;
using Musicus.Core.Common;

namespace Musicus.Core.WinRt
{
    public static class AppConstant
    {
        public const string LocalStorageAppPath = "ms-appdata:///local/";

        public const string PackageAppPath = "ms-appx:///";

        public const string MissingArtworkAppPath = PackageAppPath + "Assets/MissingArtwork.png";

        public const string SongPath = "Airstem/{0}/{1}/{2}.mp3";

        //public const string VideoPath = "Airstem/{0}.mp4";

        public const string ArtworkPath = "artworks/{0}.jpg";

        public const string ArtistsArtworkPath = "artists/{0}.jpg";

        public const string VideoArtworkPath = "videoArtwork/{0}.jpg";

        private static IBitmapImage missingArtwork;

        public static IBitmapImage MissingArtworkImage
        {
            get
            {
                return missingArtwork ?? (missingArtwork = new PclBitmapImage(new Uri(MissingArtworkAppPath)));
            }
        }

    }
}