#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite.Net;
using Musicus.Data.Collection.Model;

#endregion

namespace Musicus.Data.Collection
{
    public interface ISqlService : IDisposable
    {
        SQLiteConnection DbConnection { get; }
        bool Initialize(bool walMode = true, bool readOnlyMode = false);
        Task<bool> InitializeAsync();
        bool Error();
        bool Insert(BaseEntry entry);
        Task<bool> InsertAsync(BaseEntry entry);
        bool DeleteItem(BaseEntry item);
        Task<bool> DeleteItemAsync(BaseEntry item);
        string UpdateItem(BaseEntry item);
        Task<string> UpdateItemAsync(BaseEntry item);
        int GetQueueSize(string path);
        List<Song> GetAllTracks();
        List<Folder> GetAllFolders();
        List<Artist> GetAllArtists();
        List<Album> GetAllAlbums();
        List<Video> GetAllVideos();
        List<QueueSong> GetAllQueueSong();
        List<Playlist> GetAllPlaylists();
        List<PlaylistSong> GetAllPlaylistsTracks();
        QueueSong GetFirstQueueTrack(Func<QueueSong, bool> expression, string path);
        Song GetFirstTrack(Func<Song, bool> expression, string path);
        bool GetFirstArtist(int id, string path);
        bool GetFirstAlbum(int id, string path);
        Task DeleteTableAsync<T>();
        string GetLyrics(string songName, string artistName);
        void BeginTransaction();
        void Commit();
    }
}