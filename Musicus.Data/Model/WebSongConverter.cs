using IF.Lastfm.Core.Objects;
using Musicus.Data.Model.Deezer;
using Musicus.Data.Model.WebData;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Spotify.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Musicus.Data.Model
{
    public class WebSongConverter
    {
        public static WebSong CreateSong(LastTrack track)
        {
            var song = new WebSong()
            {
                Name = track.Name,
                Id = track.Id != null ? track.Id : track.Mbid,
                Artist = !string.IsNullOrEmpty(track.ArtistName) ? track.ArtistName : "Unknown Artist",
                Album = !string.IsNullOrEmpty(track.AlbumName) ? track.AlbumName : "Unknown Album",
            };

            if (track.Images != null)
            {
                var image = GetImage(track.Images);
                if (image != null)
                    song.ArtworkImage = image;
            }

            return song;
        }

        public static WebSong CreateSong(SimpleTrack track)
        {
            var song = new WebSong()
            {
                Name = track.Name,
                Id = track.Id,
            };


            if (track.Artist != null)
                song.Artist = !string.IsNullOrEmpty(track.Artist.Name) ? track.Artist.Name : "Unknown Artist";
            else song.Artist = "Unknown Artist";

            return song;
        }

        public static WebSong CreateSong(FullTrack track)
        {
            var song = new WebSong()
            {
                Name = track.Name,
                Id = track.Id,
            };

            if (track.Album != null)
            {
                song.Album = !string.IsNullOrEmpty(track.Album.Name) ? track.Album.Name : "Unknown Album";
                if (track.Album.Images != null)
                {
                    var image = track.Album.Images[0];
                    if (image != null) song.ArtworkImage = new Uri(image.Url, UriKind.RelativeOrAbsolute);
                }
            }

            else
                song.Album = "Unknown Album";

            if (track.Artist != null)
                song.Artist = !string.IsNullOrEmpty(track.Artist?.Name) ? track.Artist.Name : "Unknown Artist";
            else song.Artist = "Unknown Artist";

            return song;
        }

        public static WebSong CreateDeezerSong(DeezerSong track)
        {

            var song = new WebSong()
            {
                Name = track.title,
                Id = track.id + "",
            };

            if (track.album != null)
            {
                song.Album = !string.IsNullOrEmpty(track.album.title) ? track.album.title : "Unknown Album";
                if (track.album.cover != null)
                {
                    var image = track.album.cover;
                    if (image != null) song.ArtworkImage = new Uri(image, UriKind.RelativeOrAbsolute);
                }
            }

            else
                song.Album = "Unknown Album";

            if (track.artist != null)
                song.Artist = !string.IsNullOrEmpty(track.artist?.name) ? track.artist.name : "Unknown Artist";
            else song.Artist = "Unknown Artist";

            return song;
        }

        public static WebAlbum CreateAlbum(SimpleAlbum album)
        {

            var webAlbum = new WebAlbum()
            {
                Title = album.Name,
                Id = album.Id
            };

            var image = album.Images[0];
            if (image != null)
            {
                webAlbum.Artwork = new Uri(image.Url, UriKind.RelativeOrAbsolute);
                webAlbum.HasArtwork = true;
            }

            else
            {
                webAlbum.Artwork = new Uri("ms-appx:///Assets/MissingArtwork.png", UriKind.RelativeOrAbsolute);
                webAlbum.HasArtwork = false;
            }


            return webAlbum;
        }

        public static WebAlbum CreateAlbum(LastAlbum album)
        {

            var webAlbum = new WebAlbum()
            {
                Title = album.Name,
                Id = album.Id != null ? album.Id : album.Mbid,
                ReleaseDate = album.ReleaseDateUtc.DateTime
            };

            var image = album.Images;
            if (image != null)
            {
                webAlbum.Artwork = GetImage(image);
                webAlbum.HasArtwork = true;
            }

            else
                webAlbum.HasArtwork = false;

            if (!string.IsNullOrEmpty(album.ArtistName))
                webAlbum.ArtistName = album.ArtistName;

            if (album.Tracks != null)
            {
                webAlbum.Tracks = convertToList(album.Tracks.ToList(), album.Name, album.Images);
            }

            return webAlbum;
        }


        public static WebAlbum CreateDeezerAlbum(DeezerAlbum album)
        {

            var webAlbum = new WebAlbum()
            {
                Title = album.title,
                Id = album.id + ""
            };

            var image = album.cover;
            if (image != null)
            {
                webAlbum.Artwork = new Uri(image, UriKind.RelativeOrAbsolute);
                webAlbum.HasArtwork = true;
            }

            else
            {
                webAlbum.Artwork = new Uri("ms-appx:///Assets/MissingArtwork.png", UriKind.RelativeOrAbsolute);
                webAlbum.HasArtwork = false;
            }


            return webAlbum;
        }

        private static List<WebSong> convertToList(IEnumerable<LastTrack> list, string albumName, IF.Lastfm.Core.Objects.LastImageSet artwork)
        {
            List<WebSong> songs = new List<WebSong>();
            foreach (var deezerSong in list)
            {
                songs.Add(new WebSong()
                {
                    Name = deezerSong.Name,
                    Artist = deezerSong.ArtistName,
                    Id = deezerSong.Id,
                    Album = albumName,
                    ArtworkImage = GetImage(artwork),
                    IsDeezerTrack = true
                });
            }
            return songs;
        }

        static Uri GetImage(LastImageSet artwork)
        {
            try
            {
                if (string.IsNullOrEmpty(artwork?.Largest?.ToString()))
                    return artwork?.Largest;
                else if (string.IsNullOrEmpty(artwork?.Large?.ToString()))
                    return artwork?.Large;
                return artwork?.ExtraLarge;
            }
            catch
            {
                return new Uri(string.Empty);
            }
        }

    }
}
