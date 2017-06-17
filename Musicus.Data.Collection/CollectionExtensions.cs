using System;
using System.Linq;
using Musicus.Data.Collection.Model;
using Musicus.Data.Spotify.Models;
using IF.Lastfm.Core.Objects;
using Musicus.Data.Model.WebSongs;
using Windows.UI.Xaml;
using Musicus.Core.Common;
using Musicus.Data.Model;

namespace Musicus.Data.Collection
{
    public static class CollectionExtensions
    {


        public static Artist ToViezArtist(this WebSong ViezArtist)
        {
            return new Artist
            {
                ProviderId = string.Format("Viez." + ViezArtist.Id),
                Name = ViezArtist.Artist.Trim().Replace("  ", " ")
            };
        }

        public static Album ToViezAlbum(this WebSong ViezAlbum)
        {
            var album = new Album
            {
                ProviderId = string.Format("Viez." + ViezAlbum.Id),
                Name = ViezAlbum.Album.Trim().Replace("  ", " ")
            };
            return album;
        }

        public static Song ToViezSong(this WebSong ViezTrack)
        {
            var song = new Song
            {
                ProviderId = string.Format("Viez." + ViezTrack.Id),
                Name = ViezTrack.Name.Trim().Replace("  ", " "),
                AudioUrl = ViezTrack.Id
            };
            return song;
        }


        public static Artist ToArtist(this LastArtist lastArtist)
        {
            return new Artist
            {
                Name = lastArtist.Name.Trim().Replace("  ", " "),
                ProviderId = lastArtist.Id
            };
        }

        public static Album ToAlbum(this LastAlbum lastAlbum)
        {
            var album = new Album
            {
                ProviderId = lastAlbum.Id,
                Name = lastAlbum.Name.Trim().Replace("  ", " ")
            };

            return album;
        }

        public static Song ToSong(this LastTrack track)
        {
            var song = new Song
            {
                ProviderId = track.Id,
                Name = track.Name.Trim().Replace("  ", " ")
            };
            return song;
        }

        public static Artist ToArtist(this LastTrack Artist)
        {
            return new Artist
            {
                Name = Artist.ArtistName.Trim().Replace("  ", " "),
                ProviderId =
                    !string.IsNullOrEmpty(Artist.Mbid) ? ("mbid." + Artist.Mbid) : ("lastid." + Artist.Id)
            };
        }

        public static Album ToAlbum(this LastTrack Album)
        {
            var album = new Album
            {
                ProviderId =
                   !string.IsNullOrEmpty(Album.Mbid) ? ("mbid." + Album.Mbid) : ("lastid." + Album.Id),
                Name = Album.AlbumName.Trim().Replace("  ", " "),
            };

            return album;
        }

        public static Artist ToArtist(this SimpleArtist simpleArtist)
        {
            return new Artist
            {
                Name = simpleArtist.Name.Trim().Replace("  ", " "),
                ProviderId = "spotify." + simpleArtist.Id
            };
        }

        public static Album ToAlbum(this FullAlbum fullAlbum)
        {
            var album = new Album
            {
                ProviderId = "spotify." + fullAlbum.Id,
                Name = fullAlbum.Name.Trim().Replace("  ", " "),
                ReleaseDate = GetDateTime(fullAlbum)
            };

            return album;
        }

        private static DateTime GetDateTime(FullAlbum album)
        {
            if (album.ReleaseDatePrecision != "year") return DateTime.Parse(album.ReleaseDate);
            var year = int.Parse(album.ReleaseDate);
            return new DateTime(year, 1, 1);
        }

        public static Song ToSong(this SimpleTrack track)
        {
            var song = new Song
            {
                ProviderId = "spotify." + track.Id,
                Name = track.Name.Trim().Replace("  ", " "),
                TrackNumber = track.TrackNumber
            };
            return song;
        } 

        public static IncrementalObservableCollection<WebSong> ToWebSong(this IncrementalObservableCollection<LastTrack> tracks)
        {
            IncrementalObservableCollection<WebSong> _songs = new IncrementalObservableCollection<WebSong>();

            foreach (var track in tracks)
            {
               _songs.Add(WebSongConverter.CreateSong(track));
            }
            return _songs;
        }
    }
}