using Musicus.Core;
using Musicus.Data.Collection.Model;
using Musicus.Data.Collection.RunTime;
using System;
using System.IO;
using Windows.Storage;

namespace Musicus.Data.Collection
{
    public class BackgroundQueueHelper
    {
        private static string DbMainPath
        {
            get
            {
                return ApplicationData.Current.LocalFolder.Path;
            }
        }

        private static ISqlService Sql
        {
            get
            {
                return new SqlService();
            }
        }

        public static QueueSong GetQueueSong(Func<QueueSong, bool> expression)
        {
            QueueSong queueSong = Sql.GetFirstQueueTrack(expression, Path.Combine(DbMainPath, PlayerConstants.BgDataBasePath));
            if (queueSong != null)
            {
                Song song = Sql.GetFirstTrack(p => p.Id == queueSong.SongId, Path.Combine(DbMainPath, PlayerConstants.DataBasePath));
                queueSong.Song = song;
                return queueSong;
            }
            return null;
        }

        public static bool IsValidPath(int id)
        {
            return Sql.GetFirstArtist(id, Path.Combine(DbMainPath, PlayerConstants.DataBasePath));
        }

        public static bool IsValidPathForAlbum(int id)
        {
            return Sql.GetFirstAlbum(id, Path.Combine(DbMainPath, PlayerConstants.DataBasePath));
        }
    }
}
