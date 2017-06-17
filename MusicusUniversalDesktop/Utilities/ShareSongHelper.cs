using Windows.Storage;
using Musicus.Core.Utils;
using Musicus.Core.WinRt;
using Musicus.Core.WinRt.Utilities;
using Musicus.Data.Collection.Model;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using System.Collections.Generic;


namespace Musicus.Utilities
{
    public static class ShareSongHelper
    {
        private static List<IStorageFile> Items;
        public static string _filename, _path;

        public static async void ShareSong(List<Song> songs)
        {             
            Items = new List<IStorageFile>();
            foreach (var song in songs)
            {
                _filename = song.Name.CleanForFileName("Invalid Song Name");
                if (song.ArtistName != song.Album.PrimaryArtist.Name)
                    _filename = song.ArtistName.CleanForFileName("Invalid Artist Name") + "-" + _filename;

                _path = string.Format(
                AppConstant.SongPath,
                song.Album.PrimaryArtist.Name.CleanForFileName("Invalid Artist Name"),
                song.Album.Name.CleanForFileName("Invalid Album Name"),
                _filename);
                var files = await WinRtStorageHelper.GetFileAsync(_path, KnownFolders.MusicLibrary);
                Items.Add(files);
            }
            Register();
        }

        public static async void ShareVideo(Video video)
        {
            Items = new List<IStorageFile>();
            var files = await WinRtStorageHelper.GetFileAsync(video.VideoUrl, KnownFolders.VideosLibrary);
            Items.Add(files);
            Register();
        }

        private static void Register()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(ShareUIHelper);
            DataTransferManager.ShowShareUI();
        }

        private static void ShareUIHelper(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = "Sharing Media from Airstem.";
            DataRequestDeferral deferral = request.GetDeferral();
            try
            {
                request.Data.SetStorageItems(Items);
            }
            finally
            {
                deferral.Complete();
                //if (Items != null) Items = null;
            }
        }

    }
}