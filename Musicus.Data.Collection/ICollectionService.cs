#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Musicus.Core.Common;
using Musicus.Data.Collection.Model;
using TagLib;
using Windows.Storage.FileProperties;

#endregion

namespace Musicus.Data.Collection
{
    public interface ICollectionService
    {
        bool IsLibraryLoaded { get; }
        int ScaledImageSize { get; set; }
        OptimizedObservableCollection<Song> Songs { get; set; }
        OptimizedObservableCollection<Album> Albums { get; set; }
        OptimizedObservableCollection<Artist> Artists { get; set; }
        OptimizedObservableCollection<Video> Videos { get; set; }
        OptimizedObservableCollection<Folder> Folders { get; set; }
        OptimizedObservableCollection<QueueSong> CurrentPlaybackQueue { get; }
        OptimizedObservableCollection<QueueSong> PlaybackQueue { get; }
        OptimizedObservableCollection<Playlist> Playlists { get; set; }
        event EventHandler LibraryLoaded;

      
        /// <summary>
        ///     Loads all songs, albums, artist and playlists/queue.
        /// </summary>
        void LoadLibrary();
        Task LoadLibraryAsync();

        /// <summary>
        ///     Adds the song to the database and collection.
        /// </summary>
        Task AddSongAsync(Song song, Tag tags = null, bool isLastTrack = false, bool isLocalTrack = false, bool isDeezerTrack = false);
        Task AddVideoAsync(Video video, Tag tags = null, StorageItemThumbnail thumb = null);
        /// <summary>
        ///     Deletes the song from the database and collection.  Also all related files.
        /// </summary>
        Task DeleteSongAsync(Song song);
        Task DeleteVideoAsync(Video video);
        
        bool SongAlreadyExists(string name, string album, string artist);
        bool SongAlreadyExists(string localSongPath);
        bool VideoAlreadyExists(string localSongPath);
        bool VideoAlreadyExists(string title, string author);
        bool ViezSongAlreadyExists(string name, string album, string artist);

        bool CheckIfExist(int id);
        IEnumerable<Song> GetMatchingTracks(string query, int count);
        IEnumerable<Album> GetMatchingAlbums(string query, int count);
        Artist GetMatchingArtists(string query);
        IEnumerable<Video> GetMatchingVideos(string query, int count);

        IEnumerable<Song> GetLastPlayed(int count = 5);
        IEnumerable<Song> GetLastAdded(int count = 5);
        IEnumerable<Song> GetDownloads();

        QueueSong GetQueueSong(int songId);
        Artist GetArtist(int id);
        Artist GetArtistByName(string name);
        Album GetAlbum(int id);
        Playlist GetPlayList(int id);

        Task AddFolderAsync(Folder f, Windows.Storage.StorageFolder sFolder);
        Task DeleteFolderAsync(Folder f);

        bool HasArtistArtwork(int id);
        bool HasAlbumArtwork(int id);

        Uri GetArtistImage(int id);
        Uri GetAlbumImage(int id);
        Song GetSongById(int id);
        Song GetSongByArtistId(int id);
        //void SetImage();
        //void SetImageOnNavigtedTo(Playlist playlist);



        #region Playback Queue

        //void ShuffleModeChanged();
        //Task ShuffleCurrentQueueAsync();
        //Task ClearQueueAsync();
        //Task<QueueSong> AddToQueueAsync(Song song, QueueSong position = null, bool shuffleInsert = true);
        //Task DeleteFromQueueAsync(QueueSong songToRemove);
     
        Task ClearQueueAsync();
        Task<QueueSong> AddToQueueAsync(Song song, QueueSong position = null);
        Task DeleteFromQueueAsync(QueueSong songToRemove);
       
        #endregion

        #region Playlist
        Task<Playlist> CreatePlaylistAsync(string name);
        Task DeletePlaylistAsync(Playlist playlist);
        Task AddToPlaylistAsync(Playlist playlist, Song song);
        Task DeleteFromPlaylistAsync(Playlist playlist, PlaylistSong songToRemove);
        #endregion
    }
}








