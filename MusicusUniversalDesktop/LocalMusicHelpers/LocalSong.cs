using Musicus.Data.Collection.Model;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage.FileProperties;

namespace Musicus.Data.Model
{
    public class LocalSong
    {
        public LocalSong(string title, string artist, string album, string albumArtist,string radioId, string cloudid)
        {
            Title = CleanText(title);
            ArtistName = CleanText(artist);
            AlbumName = CleanText(album);
            AlbumArtist = CleanText(albumArtist);

            if (!string.IsNullOrEmpty(radioId))
                RadioId = int.Parse(radioId);
            if (!string.IsNullOrEmpty(cloudid))
                CloudId = cloudid;

            if (!string.IsNullOrEmpty(ArtistName) || !string.IsNullOrEmpty(AlbumArtist))
                ArtistId = Convert.ToBase64String(Encoding.UTF8.GetBytes((AlbumArtist ?? ArtistName).ToLower()));
            if (!string.IsNullOrEmpty(AlbumName))
                AlbumId = Convert.ToBase64String(Encoding.UTF8.GetBytes(AlbumName.ToLower()));
            if (!string.IsNullOrEmpty(Title))
                Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(Title.ToLower())) + ArtistId + AlbumId;
        }

        public LocalSong(MusicProperties musicProps)
        {
            Title = CleanText(musicProps.Title);
            AlbumName = CleanText(musicProps.Album);
            AlbumArtist = CleanText(musicProps.AlbumArtist);
            ArtistName = CleanText(musicProps.Artist);

            RadioId = -1;
            BitRate = (int)musicProps.Bitrate;
            Duration = musicProps.Duration;
            Genre = musicProps.Genre.FirstOrDefault();
            TrackNumber = (int)musicProps.TrackNumber;
          
            if (!string.IsNullOrEmpty(ArtistName) || !string.IsNullOrEmpty(AlbumArtist))
                ArtistId = Convert.ToBase64String(Encoding.UTF8.GetBytes((AlbumArtist ?? ArtistName).ToLower()));
            if (!string.IsNullOrEmpty(AlbumName))
                AlbumId = Convert.ToBase64String(Encoding.UTF8.GetBytes(AlbumName.ToLower()));
            if (!string.IsNullOrEmpty(Title))
                Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(Title.ToLower())) + ArtistId + AlbumId;

            //if (musicProps.Rating > 1)
            //    HeartState = HeartState.Like;
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            text = Regex.Replace(text, @"/^[\x20-\x7D]+$/", "");
            // convert multiple spaces into one space   
            return Regex.Replace(text, @"\s +", " ").Trim();
        }

        public string Id { get; set; }
        public string ArtistId { get; set; }
        public string AlbumId { get; set; }

        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string AlbumArtist { get; set; }
        public string Genre { get; set; }
        public string FilePath { get; set; }
        public int BitRate { get; set; }
        public int TrackNumber { get; set; }
        public int PlayCount { get; set; }
        public int RadioId { get; set; }
        public string CloudId { get; set; }
        public Uri ArtworkImage { get; set; }
        public TimeSpan Duration { get; set; }
    }


    public class LocalVideo
    {

        public LocalVideo(VideoProperties videoProps)
        {
            Title = CleanText(videoProps.Title);
            if (!string.IsNullOrEmpty(videoProps.Publisher))
                Author = CleanText(videoProps.Publisher);
            Duration = videoProps.Duration;
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            text = Regex.Replace(text, @"/^[\x20-\x7D]+$/", "");
            // convert multiple spaces into one space   
            return Regex.Replace(text, @"\s +", " ").Trim();
        }

        public string Title { get; set; }
        public string Author { get; set; }
        public TimeSpan Duration { get; set; }
        public string FilePath { get; set; }
    }


    public static class LocalExtentions
    {
        public static Video ToVideo(this LocalVideo video)
        {
            return new Video
            {
                Title = video.Title,
                Author = video.Author,
                VideoUrl = video.FilePath,
                VideoState = VideoState.Local,
                ViewCount = 0,
                Duration = (video.Duration != null) ? video.Duration : TimeSpan.Zero,
                AddedOn = DateTime.Now.ToString("dd/MM/yyyy")
            };
        }

        public static Album ToAlbum(this LocalSong track)
        {
            return new Album
            {
                ProviderId = "local." + track.AlbumId,
                Name = track.AlbumName.Trim()
            };
        }

        public static Artist ToArtist(this LocalSong track)
        {
            return new Artist
            {
                ProviderId = "local." + track.ArtistId,
                Name = (string.IsNullOrEmpty(track.AlbumArtist)
                    ? track.ArtistName
                    : track.AlbumArtist).Trim()
            };
        }

        public static Song ToSong(this LocalSong track)
        {
            var song = new Song
            {
                ProviderId = "local." + track.Id,
                Name = track.Title,
                ArtistName = track.ArtistName,
                Duration = track.Duration,
                AudioUrl = track.FilePath,
                SongState = SongState.Local,
                TrackNumber = track.TrackNumber,
                PlayCount = track.PlayCount
            };

            if (!string.IsNullOrEmpty(track.ArtistId))
            {
                song.Artist = track.ToArtist();
                if (string.IsNullOrEmpty(song.ArtistName))
                {
                    song.ArtistName = song.Artist.Name;
                }
            }

            if (!string.IsNullOrEmpty(track.AlbumId))
            {
                song.Album = track.ToAlbum();
                song.Album.PrimaryArtist = song.Artist;
            }
            return song;
        }
    
    }

}
