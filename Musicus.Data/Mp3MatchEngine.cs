#region
using Musicus.Core.Utils;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Musicus.Data
{
    public enum Mp3Provider
    {
        ProstoPleer,
        Songily,
        Mp3FreeXMatch,
        Mp3Lio,      
        Mp3Pm,
        YouTube,
        Mp3Miner,
        SoundCloud,
        AnyMaza,
        MiMp3,
        Mp3Skull,
        Mp3Clan,
        Netease,
        Mp3Truck,
        Mp3Glu,
        Meile
    }


    public class Mp3MatchEngine
    {
        private readonly Mp3Provider[] _providers =
        {

            Mp3Provider.Mp3Lio,
            Mp3Provider.Mp3Pm,
            Mp3Provider.YouTube,
            Mp3Provider.SoundCloud,
            Mp3Provider.Mp3Glu,

            Mp3Provider.Mp3FreeXMatch,
            
            Mp3Provider.Mp3Skull,

            Mp3Provider.ProstoPleer,
            Mp3Provider.Songily,

            Mp3Provider.Mp3Miner,

            Mp3Provider.AnyMaza,
            Mp3Provider.MiMp3,
           

            Mp3Provider.Mp3Clan,
            Mp3Provider.Netease,
            Mp3Provider.Mp3Truck,
            
            Mp3Provider.Meile,

        };

        private readonly IMp3SearchService _service;
        public Mp3MatchEngine(IMp3SearchService mp3Search)
        {
            _service = mp3Search;
        }

        public async Task<WebSong> FindMp3For(string title, string artist)
        {
            title.CleanUpAsync();
            artist.ToCleanQuery();
            int currentProvider = 0;
            WebSong song = new WebSong();

            while (currentProvider < _providers.Length)
            {
                try
                {
                    song = await GetMatch(_providers[currentProvider], title, artist).ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
                if (song != null) break;
                currentProvider++;
            }

            return song;
        }


        public async Task<WebSong> FindMp3ByProvider(string title, string artist, int id)
        {
            WebSong song = new WebSong();
            try
            {
                song = await GetMatch(_providers[id-1], title, artist).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
            if (song != null)
                return song;
            else
                return await FindMp3For(title, artist);
        }



        public async Task<WebSong> GetMatch(Mp3Provider provider, string title, string artist, string album = null)
        {
            var webSongs = new List<WebSong>();
            WebSong song = null;

            switch (provider)
            {
                case Mp3Provider.ProstoPleer:
                    webSongs = await _service.SearchPleer(title, artist).ConfigureAwait(false);
                    break;

                case Mp3Provider.Songily:
                    webSongs = await _service.SearchSongily(title, artist).ConfigureAwait(false);
                    break;

                case Mp3Provider.Mp3FreeXMatch:
                    webSongs = await _service.SearchMp3Freex(title, artist).ConfigureAwait(false);
                   break;

                case Mp3Provider.Mp3Lio:
                    webSongs = await _service.SearchMp3lio(title, artist).ConfigureAwait(false);
                   break;

                case Mp3Provider.Mp3Pm:
                    webSongs = await _service.SearchMp3Pm(title, artist).ConfigureAwait(false);
                   break;

                case Mp3Provider.YouTube:
                    webSongs = await _service.SearchYoutube(title, artist).ConfigureAwait(false);
                   break;
          
                case Mp3Provider.Mp3Miner:
                    webSongs = await _service.SearchMp3Miner(title, artist).ConfigureAwait(false);
                   break;

                case Mp3Provider.SoundCloud:
                    webSongs = await _service.SearchSoundCloud(title, artist).ConfigureAwait(false);
                   break;

                case Mp3Provider.AnyMaza:
                    webSongs = await _service.SearchAnyMaza(title, artist).ConfigureAwait(false);
                    break;

                case Mp3Provider.MiMp3:
                    webSongs = await _service.SearchMiMp3(title, artist).ConfigureAwait(false);
                   break;

                case Mp3Provider.Mp3Skull:
                    webSongs = await _service.SearchMp3Skull(title, artist).ConfigureAwait(false);
                    break;

                case Mp3Provider.Mp3Clan:
                    webSongs = await _service.SearchMp3Clan(title, artist).ConfigureAwait(false);
                    break;

                case Mp3Provider.Netease:
                    webSongs = await _service.SearchNetease(title, artist).ConfigureAwait(false);
                    break;

                case Mp3Provider.Mp3Truck:
                    webSongs = await _service.SearchMp3Truck(title, artist).ConfigureAwait(false);
                    break;

                case Mp3Provider.Mp3Glu:
                    webSongs = await _service.SearchMp3Glu(title, artist).ConfigureAwait(false);
                    break;

                case Mp3Provider.Meile:
                    webSongs = await _service.SearchMeile(title, artist).ConfigureAwait(false);
                    break;

            }

            if (webSongs != null)
            {
                song = webSongs.FirstOrDefault(p => p.IsBestMatch);
                if (song != null)
                    return song;
                song = webSongs.FirstOrDefault(p => p.IsMatch && !p.IsLinkDeath);
                if (song != null)
                    return song;

            }
            return null;
        }
    }
}