

using IF.Lastfm.Core.Objects;
using Musicus.Core.Utils;
using Musicus.Core.WinRt.Common;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Spotify.Models;
using Musicus.Helpers.SaveTrackHelpers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Musicus.Helpers
{
    public class SongSavingHelper
    {
        static int _warningCount = 0;
        public static async Task SaveViezTrackLevel1(WebSong track)
        {
            if (!App.Locator.Setting.IsLoggedIn)
            {
                if (_warningCount > 0)
                {
                    ToastManager.ShowError("Login required.");
                    return;
                }
                await MessageHelpers.LoginRequired();
                _warningCount += 1;
            }

            
            var fTrack = track;
            App.Locator.PBar.IsEnable = true;
            try
            {
                track.Name = track.Name.GetSongNameFromMain();
                track.Artist = track.Artist.GetArtistName();

                if (track.Name == "Unknown Track")
                    track.Name = fTrack.Name;
                if (track.Artist == "Unknown Artist" && fTrack.Artist != "Unknown Artist")
                    track.Artist = fTrack.Artist;

                await ViezTrackSavingHelper.SaveViezTrackLevel1(track);
            }
            catch
            {
                await ViezTrackSavingHelper.SaveViezTrackLevel1(track);
            }

            App.Locator.PBar.IsEnable = false;
        }


        public static async Task SaveLastTrackLevel1(LastTrack track)
        {
            if (!App.Locator.Setting.IsLoggedIn)
            {
                if (_warningCount > 0)
                {
                    ToastManager.ShowError("Login required.");
                    return;
                }
                await MessageHelpers.LoginRequired();
                _warningCount += 1;
            }
            var fTrack = track;
            App.Locator.PBar.IsEnable = true;
            try
            {
                track.Name = track.Name.GetSongNameFromMain();
                track.ArtistName = track.ArtistName.GetArtistName();
                if (track.Name == "Unknown Track")
                    track.Name = fTrack.Name;
                if (track.ArtistName == "Unknown Artist" && fTrack.ArtistName != "Unknown Artist")
                    track.ArtistName = fTrack.ArtistName;
                await LastFmTrackSavingHelper.SaveLastTrackLevel1(track);
            }
            catch
            {
                await LastFmTrackSavingHelper.SaveLastTrackLevel1(track);
            }
            App.Locator.PBar.IsEnable = false;
        }


        public static async Task SaveSpotifyChartTrackLevel1(ChartTrack track)
        {
            if (!App.Locator.Setting.IsLoggedIn)
            {
                if (_warningCount > 0)
                {
                    ToastManager.ShowError("Login required.");
                    return;
                }
                await MessageHelpers.LoginRequired();
                _warningCount += 1;
            }
            var fTrack = track;
            App.Locator.PBar.IsEnable = true;
            try
            {
                track.Name = track.Name.GetSongNameFromMain();
                track.ArtistName = track.ArtistName.GetArtistName();
                if (track.Name == "Unknown Track" )
                    track.Name = fTrack.Name;
                if (track.ArtistName == "Unknown Artist" && fTrack.ArtistName != "Unknown Artist")
                    track.ArtistName = fTrack.ArtistName;

                await SpotifySavingHelper.SaveSpotifyChartTrackLevel1(track);
            }
            catch
            {
                await SpotifySavingHelper.SaveSpotifyChartTrackLevel1(track);
            }
            App.Locator.PBar.IsEnable = false;
        }


        public static async Task SaveSpotifyTrackLevel1(FullTrack track, bool manualMatch = false)
        {
            if (!App.Locator.Setting.IsLoggedIn)
            {
                if (_warningCount > 0)
                {
                    ToastManager.ShowError("Login required.");
                    return;
                }
                await MessageHelpers.LoginRequired();
                _warningCount += 1;
            }
            var fTrack = track;
            App.Locator.PBar.IsEnable = true;
            try
            {
                track.Name = track.Name.GetSongNameFromMain();
                track.Artist.Name = track.Artist.Name.GetArtistName();

                if (track.Name == "Unknown Track")
                    track.Name = fTrack.Name;
                if (track.Artist.Name == "Unknown Artist" && fTrack.Artist.Name != "Unknown Artist")
                    track.Artist.Name = fTrack.Artist.Name;

                await SpotifySavingHelper.SaveSpotifyTrackLevel1(track);
            }
            catch
            {
                await SpotifySavingHelper.SaveSpotifyTrackLevel1(track);
            }
            App.Locator.PBar.IsEnable = false;
        }

        public static async Task SaveSpotifyTrackLevel2(SimpleTrack track, FullAlbum album)
        {
            if (!App.Locator.Setting.IsLoggedIn)
            {
                if (_warningCount > 0)
                {
                    ToastManager.ShowError("Login required.");
                    return;
                }
                await MessageHelpers.LoginRequired();
                _warningCount += 1;
            }
            var fTrack = track;
            App.Locator.PBar.IsEnable = true;
            try
            {
                track.Name = track.Name.GetSongNameFromMain();
                track.Artist.Name = track.Artist.Name.GetArtistName();

                if (track.Name == "Unknown Track")
                    track.Name = fTrack.Name;
                if (track.Artist.Name == "Unknown Artist" && fTrack.Artist.Name != "Unknown Artist")
                    track.Artist.Name = fTrack.Artist.Name;

                await SpotifySavingHelper.SaveSpotifyTrackLevel2(track, album);
            }
            catch
            {
                await SpotifySavingHelper.SaveSpotifyTrackLevel2(track, album);
            }
            App.Locator.PBar.IsEnable = false;
        }

        public static async Task SaveSpotifyAlbumLevel1(FullAlbum album)
        {
            if (!App.Locator.Setting.IsLoggedIn)
            {
                if (_warningCount > 0)
                {
                    ToastManager.ShowError("Login required.");
                    return;
                }
                await MessageHelpers.LoginRequired();
                _warningCount += 1;
            }
            App.Locator.PBar.IsEnable = true;
            try
            {           
                if (album != null)
                {
                    foreach (var obj in album.Tracks.Items)
                    {
                        var fTrack = obj;
                        obj.Name = obj.Name.GetSongNameFromMain();
                        obj.Artist.Name = obj.Artist.Name.GetArtistName();

                        if (obj.Name == "Unknown Track")
                            obj.Name = fTrack.Name;
                        if (obj.Artist.Name == "Unknown Artist" && fTrack.Artist.Name != "Unknown Artist")
                            obj.Artist.Name = fTrack.Artist.Name;
                    }
                }

                await SpotifySavingHelper.SaveSpotifyAlbumLevel1(album);           
            }

            catch
            {
                await SpotifySavingHelper.SaveSpotifyAlbumLevel1(album);
            } 

            App.Locator.PBar.IsEnable = false;
        }

        public static string ToCleanQuery(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            // invalid chars           
            text = Regex.Replace(text, @"/^[\x20-\x7D]+$/", "");
            // convert multiple spaces into one space   
            text = Regex.Replace(text, @"\s +", " ").Trim();
            return text;
        }



    }
}
