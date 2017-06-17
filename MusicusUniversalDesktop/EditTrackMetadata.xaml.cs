
using Musicus.Helpers;
using Windows.UI.Xaml.Controls.Primitives;
using System;
using IF.Lastfm.Core.Objects;
using Musicus.Data.Spotify.Models;
using Musicus.Data.Model.WebSongs;

namespace Musicus
{
    public sealed partial class EditTrackMetadata : IModalSheetPage
    {
        public EditTrackMetadata()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "METADATA";
                MainPivotItem1.Header = "edit";
            }
        }

        private object PassingValue;
        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Popup = popup;
            PassingValue = ModalSheetUtility.PassingObject;
            Load();
         
        }

     
        public void OnClosed()
        {
            Popup = null;
            Close();
        }

        private void Load()
        {
            var item = PassingValue;
            if (item is WebSong)
            {             
                TrackTitle.Text = (item as WebSong).Name;
                ArtistTitle.Text = (item as WebSong).Artist;
                AlbumTitle.Text = string.IsNullOrEmpty((item as WebSong).Album) ? "Unknown Album" : (item as WebSong).Album;
            }
            else if(item is FullTrack)
            {
                TrackTitle.Text = (item as FullTrack).Name;
                ArtistTitle.Text = (item as FullTrack).Artist.Name;
                AlbumTitle.Text = (item as FullTrack).Album.Name;
            }
            else if (item is SimpleTrack)
            {
                TrackTitle.Text = (item as SimpleTrack).Name;
                ArtistTitle.Text = (item as SimpleTrack).Artist.Name;
                AlbumTitle.Text = "Unknown Album";
            }
            else if (item is LastTrack)
            {
                TrackTitle.Text = (item as LastTrack).Name;
                ArtistTitle.Text = (item as LastTrack).ArtistName;
                AlbumTitle.Text = string.IsNullOrEmpty((item as LastTrack).AlbumName) ? "Unknown Album" : (item as LastTrack).AlbumName;
            }
        }

        private void Save()
        {
            var item = PassingValue;
            if (item is WebSong)
            {
                (item as WebSong).Name = TrackTitle.Text;
                (item as WebSong).Artist = ArtistTitle.Text;
                (item as WebSong).Album = AlbumTitle.Text;
            }

            else if (item is FullTrack)
            {
                (item as FullTrack).Name = TrackTitle.Text;
                (item as FullTrack).Artist.Name = ArtistTitle.Text;
                (item as FullTrack).Album.Name = AlbumTitle.Text;
            }

            else if (item is SimpleTrack)
            {
                (item as SimpleTrack).Name = TrackTitle.Text;
                (item as SimpleTrack).Artist.Name = ArtistTitle.Text;
               // (item as FullTrack).Album.Name = AlbumTitle.Text;
            }

            else if (item is LastTrack)
            {
                (item as LastTrack).Name = TrackTitle.Text;
                (item as LastTrack).ArtistName = ArtistTitle.Text;
                (item as LastTrack).AlbumName = AlbumTitle.Text;
            }
        }

        private void Close()
        {
            
            TrackTitle.Text = string.Empty;
            ArtistTitle.Text = string.Empty;
            AlbumTitle.Text = string.Empty;
        }

        private void Cancel(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SheetUtility.CloseEditTrackMetadataPage();
        }

        private async void Download(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TrackTitle.Text) || string.IsNullOrEmpty(AlbumTitle.Text) ||
                       string.IsNullOrEmpty(ArtistTitle.Text) || string.IsNullOrWhiteSpace(TrackTitle.Text)
                       || string.IsNullOrWhiteSpace(AlbumTitle.Text) || string.IsNullOrWhiteSpace(ArtistTitle.Text))

            {
                Core.WinRt.Common.ToastManager.ShowError("Fill in all details.");
                return;
            }

            else
            {
                Save();
                SheetUtility.CloseEditTrackMetadataPage();
                await System.Threading.Tasks.Task.Delay(200);
                if (PassingValue is WebSong)
                    await SongSavingHelper.SaveViezTrackLevel1(PassingValue as WebSong);
                else if (PassingValue is FullTrack)
                    await SongSavingHelper.SaveSpotifyTrackLevel1(PassingValue as FullTrack);
                else if (PassingValue is LastTrack)
                    await SongSavingHelper.SaveLastTrackLevel1(PassingValue as LastTrack);
            }
        }
    }
}
