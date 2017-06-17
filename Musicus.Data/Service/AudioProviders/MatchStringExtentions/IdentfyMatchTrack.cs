
using Musicus.Core.Extensions;
using Musicus.Core.Utils;
using Musicus.Data.Extensions;
using Musicus.Data.Model.WebSongs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Musicus.Data.Service.AudioProviders.MatchStringExtentions
{
    public class IdentfyMatchTrack
    {

        public static async Task<List<WebSong>> IdentifyMatches(List<WebSong> songs, string title, string artist, bool checkAll)
        {
           return await IdentifyMatchesAsync(songs, title, artist, checkAll);
        }


        private static async Task<List<WebSong>> IdentifyMatchesAsync(List<WebSong> songs, string title, string artist, bool checkAll)
        {
            var webSongs = songs?.OrderByDescending(p => p.Duration.Minutes).ToList();
            if (webSongs == null) return null;

            var sanitizedTitle = title.ToMusicusSlug();
            var sanitizedArtist = artist.ToMusicusSlug();

            foreach (var webSong in webSongs)
            {
                var matchTitle = webSong.Name.ToMusicusSlug();
                var matchArtist = webSong.Artist.ToMusicusSlug();

                if (string.IsNullOrEmpty(matchTitle)) continue;


                bool isValidated = IsFlexibleTypeValid(sanitizedTitle, matchTitle, "edition")
                                    && IsTypeValid(sanitizedTitle, matchTitle, "acapella")
                                    && IsTypeValid(sanitizedTitle, matchTitle, "acoustic")
                                    && IsTypeValid(sanitizedTitle, matchTitle, "cover")
                                    && IsTypeValid(sanitizedTitle, matchTitle, "instrumental", "karaoke")
                                    && IsTypeValid(sanitizedTitle, matchTitle, "live", "concert", "arena")
                                    && IsTypeValid(sanitizedTitle, matchTitle, "preview", "snipped")
                                    && IsTypeValid(sanitizedTitle, matchTitle, "radio")
                                    && IsTypeValid(sanitizedTitle, matchTitle, "remix", "mix", "rmx")
                                    && IsTypeValid(sanitizedTitle, matchTitle, "slowed", "slow down", "slow mo");

                if (!isValidated) continue;
               
                if(!string.IsNullOrEmpty(matchTitle) && matchTitle.Contains(sanitizedTitle))
                    webSong.IsMatch = true;

                //var isCorrectTitle = IsCorrectTitle(sanitizedTitle, sanitizedArtist, matchTitle, matchArtist);
                //if (!isCorrectTitle) continue;

                //bool isCorrectArtist = matchArtist != null
                //    ? matchArtist.Contains(sanitizedArtist) || sanitizedArtist.Contains(matchArtist)
                //    : matchTitle.Contains(sanitizedArtist)
                //      ||
                //      (webSong.FileAuthor != null &&
                //       webSong.FileAuthor.ToLower().Contains(sanitizedArtist.Replace(" ", "")));
                //if (!isCorrectArtist) continue;
            }



            var filterSongs = webSongs.Where(p => p.IsMatch).ToList();
            foreach (var webSong in (checkAll ? filterSongs : webSongs)
                    .Where(webSong => !string.IsNullOrEmpty(webSong.AudioUrl)
                                      && webSong.AudioUrl.StartsWith("http")))
            {
                if (await IsUrlOnlineAsync(webSong).DontMarshall())
                    webSong.IsBestMatch = webSong.IsMatch;
                else
                    webSong.IsLinkDeath = true;
            }

            //var mostUsedMinute =
            //    filterSongs.Where(p => !p.IsLinkDeath).GetMostUsedOccurrenceWhileIgnoringZero(p => p.Duration.Minutes);
            //webSongs.Where(p => p.IsBestMatch).ForEach(p => p.IsBestMatch = p.Duration.Minutes == mostUsedMinute);

            return webSongs;
        }


        private const int MatchTitleLenghtThreshold = 30;
        private static bool IsCorrectTitle(string sanitizedTitle, string sanitizedArtist, string matchTitle, string matchArtist)
        {
            var titleDiff = System.Math.Abs(sanitizedTitle.Length - matchTitle.Length);
            var correct = (matchTitle.Contains(sanitizedTitle) || sanitizedTitle.Contains(matchTitle))
                          &&
                          titleDiff <=
                          MatchTitleLenghtThreshold + (string.IsNullOrEmpty(matchArtist) ? sanitizedArtist.Length : 0);

            if (correct) return true;

            // this is a workaround for songs that utilize "with {artist}" instead of "ft {artist}"
            var songHasFt = sanitizedTitle.Contains(" ft ");
            var matchHasFt = matchTitle.Contains(" ft ");

            if (songHasFt == matchHasFt) return false;

            if ((songHasFt || !sanitizedTitle.Contains(" with ")) && (matchHasFt || !matchTitle.Contains(" with ")))
                return false;

            sanitizedTitle = sanitizedTitle.Replace(" with ", " ft ");
            matchTitle = matchTitle.Replace(" with ", " ft ");
            correct = IsCorrectTitle(sanitizedTitle, sanitizedArtist, matchTitle, matchArtist);

            return correct;
        }



        private static async Task<bool> IsUrlOnlineAsync(WebSong song)
        {
            try
            {
              
                song.OnlyDownload = false;
                using (var response = await song.AudioUrl.ToUri().HeadAsync())
                {
                    if (response == null || !response.IsSuccessStatusCode) return false;

                    var type = response.Content.Headers.ContentType?.MediaType ??
                                     response.Content.Headers.GetValues("Content-Type")?.FirstOrDefault() ?? "";
                    if (!type.Contains("audio") && !type.Contains("octet-stream"))
                        return false;

                    var size = response.Content.Headers.ContentLength;
                    if (size != null)
                    {
                        song.ByteSize = (long)size;
                    }
                    return song.ByteSize > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        //query

        public static string CreateQuery(string title, string artist, bool urlEncode = true)
        {
            var query = ((title + " " + artist).Trim()).Trim();
            return urlEncode ? System.Net.WebUtility.UrlEncode(query) : query;
        }


        //validators

        private static bool IsFlexibleTypeValid(string name, string matchName, string type)
        {
            name = name.ToLower();
            matchName = matchName.ToLower();
            type = type.ToLower();

            var isType = IsNameValidType(matchName, type);
            var isSupposedType = IsNameValidType(name, type);
            return isSupposedType || !isType;
        }

        private static bool IsTypeValid(string name, string matchName, params string[] types)
        {
            name = name.ToLower();
            matchName = matchName.ToLower();
            var isSupposedType = false;
            var isType = false;

            foreach (var loweredType in types.Select(type => type.ToLower()))
            {
                if (!isType)
                    isType = IsNameValidType(matchName, loweredType);

                if (!isSupposedType)
                    isSupposedType = IsNameValidType(name, loweredType);

                if (isSupposedType && isType)
                    return true;
            }

            return !isSupposedType && !isType;
        }

        private static bool IsNameValidType(string name, string type)
        {
            return name.Contains($" {type} ") || name.EndsWith($" {type}") || name.StartsWith($"{type} ")
                   || name.Contains($"({type} ") || name.Contains($" {type})")
                   || name.Contains($"\"{type}\"")
                   || name.Contains($"'{type}'")
                   || name.Contains($"{type}*")
                   || name.Contains($"*{type}")
                   || name.Contains($" {type},")
                   || name.Contains($" {type};");
        }

    }
}
