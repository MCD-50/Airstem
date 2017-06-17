using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization.Formatters;

namespace Musicus.Core.Utils
{
    public static class StringExtensions
    {
        public static string ToCleanQuery(this string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var str = DiacritisHelper.Remove(WebUtility.HtmlDecode(text.ToLower()));
            // invalid chars           
            str = Regex.Replace(str, @"[^A-Za-z0-9\s]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();

            return str;
        }

        public static async Task<bool> IsValid(this string url)
        {
            WebRequest webRequest = WebRequest.Create(url);
            WebResponse webResponse;
            try
            {
                webResponse = await webRequest.GetResponseAsync();
            }
            catch (Exception) //If exception thrown then couldn't get response from address
            {
                return false;
            }
            return true;
        }

        public static string RemoveUnwantedChars( string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            // invalid chars           
            text = Regex.Replace(text, @"/^[\x20-\x7D]+$/", "");
            // convert multiple spaces into one space   
            text = Regex.Replace(text, @"\s +", " ").Trim();
            return text;
        }

        public static string GetFineLength(this string length)
        {
            string newString = length.Replace("PT", string.Empty).Trim();

            if (newString.Contains("H"))
                newString = newString.Replace("H", ":").Trim();
            else
                newString = string.Concat("0:", newString);

            if (newString.Contains("M"))
                newString = newString.Replace("M", ":").Trim();
            else
                newString = string.Concat("0:", newString);

            newString = newString.Replace("S", string.Empty).Trim();
            return newString;
        }

        public static string GetSongNameFromMain(this string title)
        {
            string newtitle = RemoveUnwantedChars(title).ToLower();
            string finalTitle = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(newtitle))
                {

                    string[] removeBraces = newtitle.Split('-','(', ')', '|', '[', ']', '{', '}');
                    if(string.IsNullOrEmpty(removeBraces[0])) return "Unknown Track";

                    finalTitle = ApplyFilters(removeBraces[0]);
                    if (finalTitle.Length > 1)
                        return finalTitle;

                    return "Unknown Track";
                }
                return "Unknown Track";
            }

            catch 
            {
                return newtitle;
            }
        }


        public static string GetSongName(this string title)
        {
            string newtitle = RemoveUnwantedChars(title).ToLower();
            string finalTitle = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(newtitle))
                {
                    string[] words = newtitle.Split('-');
                    if (words.Length == 1)
                        return words[0];

                    if (words[1].Length > 1)
                    {
                        string[] removeBraces = words[1].Split('(',')','|','[',']','{','}');
                        finalTitle = ApplyFilters(removeBraces[0]);
                        if (finalTitle.Length > 1)
                           return finalTitle;
                    }
                   
                    return "Unknown Track";
                }
                return "Unknown Track";
            }

            catch 
            {
                return newtitle;
            }
        }


        private static string ApplyFilters(string testtitle,bool isTrack = true)
        {
            string newtitle = testtitle;
            if (newtitle.Contains("vevo")) newtitle.Replace("vevo", string.Empty);
            if (newtitle.Contains("video")) newtitle.Replace("video", string.Empty);
            if (newtitle.Contains("audio")) newtitle.Replace("audio", string.Empty);
            if (newtitle.Contains("offcial")) newtitle.Replace("offcial", string.Empty);
            if (newtitle.Contains("youtube")) newtitle.Replace("youtube", string.Empty);
            if (newtitle.Contains("full")) newtitle.Replace("full", string.Empty);
            if (newtitle.Contains("preview")) newtitle.Replace("preview", string.Empty);
            if (newtitle.Contains("music")) newtitle.Replace("music", string.Empty);
            if (newtitle.Contains("lyrics")) newtitle.Replace("lyrics", string.Empty);
            if (newtitle.Contains("new")) newtitle.Replace("new", string.Empty);
            if (newtitle.Contains("full")) newtitle.Replace("full", string.Empty);
            if (newtitle.Contains("hd")) newtitle.Replace("hd", string.Empty);
            if (!string.IsNullOrEmpty(newtitle))
            {
                string finalTitle = Converters.FirstLetterUpperCaseCoverter.FirstCharToUpper(newtitle);
                return finalTitle;
            }
            return isTrack ? "Unknown Track" : "Unknown Artist";
        }

        public static string GetArtistName(this string title)
        {
            string newtitle = RemoveUnwantedChars(title).ToLower();
            string finalTitle = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(newtitle))
                {
                    string[] words = newtitle.Split('-');
                    if (words[0].Length > 1)
                    {
                        string[] removeBraces = words[0].Split(',','&','(', ')');
                        finalTitle = ApplyFilters(removeBraces[0], isTrack: false);
                        if (finalTitle.Length > 1)
                            return finalTitle;
                    }

                    return "Unknown Artist";
                }
                return "Unknown Artist";
            }

            catch 
            {
                return newtitle;
            }
        
        }


        //private static bool CheckTitle(string title)
        //{
        //    bool contain = false;
        //    var charArray = title.Trim().ToCharArray();
        //    foreach (char item in charArray)
        //    {
        //        if (item > 127)
        //        {
        //            contain = true;
        //            break;
        //        }
        //        else
        //            contain = false;
        //    }
        //    if (!contain)
        //        return false;
        //    else return true;
        //}



        public static string CleanForFileName(this string str, string invalidMessage)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            if (str.Length > 35)
            {
                str = str.Substring(0, 35);
            }

            str = str.ForceValidEnding();

            /*
             * A filename cannot contain any of the following characters:
             * \ / : * ? " < > |
             */
            var name =
                str.Replace("\\", string.Empty)
                    .Replace("/", string.Empty)
                    .Replace(":", " ")
                    .Replace("*", string.Empty)
                    .Replace("?", string.Empty)
                    .Replace("\"", "'")
                    .Replace("<", string.Empty)
                    .Replace(">", string.Empty)
                    .Replace("|", " ");

            return string.IsNullOrEmpty(name) ? invalidMessage : name;
        }

        public static string ToMusicusSlug(this string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var str = WebUtility.HtmlDecode(text.ToLower());
            str = str.Replace(" and ", " ").Replace("feat", "ft");

            str = str.ToUnaccentedText();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();

            return str;
        }

        public static string CleanUpAsync(this string obj)
        {
            string sanitizedTitle = obj.ToLower().Replace("feat.", "ft.") // better alternatives for matching
                            .Replace("- bonus track", string.Empty)
                            .Replace("bonus track", string.Empty)
                            .Replace("- live", "(live)")
                            .Replace("- remix", "(remix)")
                            .Replace("a cappella", "acappella")
                            .Replace("- acoustic version", "(acoustic)")
                            .Replace("- acoustic", "(acoustic)")
                            .Replace("- cover", "(cover)")
                            .Replace("- stereo", string.Empty)
                            .Replace("- mono", string.Empty)
                            .Replace("- intro", string.Empty)
                            .Replace("- no intro", string.Empty)
                            .Replace("- ep version", string.Empty)
                            .Replace("- deluxe edition", string.Empty);

            //multiple sapace to one space....
            sanitizedTitle = Regex.Replace(sanitizedTitle, @"\s+", " ").Trim();

            if (sanitizedTitle.Contains("- from the") && sanitizedTitle.EndsWith("soundtrack"))
                sanitizedTitle = sanitizedTitle.Substring(0, sanitizedTitle.IndexOf("- from the") - 1);

            return sanitizedTitle;
        }

        public static string ToUnaccentedText(this string accentedString)
        {
            return string.IsNullOrEmpty(accentedString) ? accentedString : DiacritisHelper.Remove(accentedString);
        }

        public static string ForceValidEnding(this string str)
        {
            var isNonAccepted = true;

            while (isNonAccepted)
            {
                var lastChar = str[str.Length - 1];

                isNonAccepted = lastChar == ' ' || lastChar == '.' || lastChar == ';' || lastChar == ':';

                if (isNonAccepted) str = str.Remove(str.Length - 1);
                else break;

                if (str.Length == 0) return str;

                isNonAccepted = lastChar == ' ' || lastChar == '.' || lastChar == ';' || lastChar == ':';
            }

            return str;
        }

        public static async Task<T> DeserializeAsync<T>(this string json)
        {
            return await Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                    catch
                    {
                        return default(T);
                    }
                }).ConfigureAwait(false);
        }


        public static string Tokenize(this string[] values)
        {
            // the delimiter used for tokenizing is a space, let's encode it.
            // Encode % and then we can encode space, without worring if the original string had %.
            var encoded = values.Select(p => p?.Replace("%", "%25").Replace(" ", "%20"));

            // Join using the space delimiter
            return string.Join(" ", encoded);
        }

        /// <summary>
        ///     DeTokenizes the specified values.
        ///     "test yo%20testing" =&gt; ["test", "yo testing"]
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public static string[] DeTokenize(this string token)
        {
            // reverse the proccess of encoding
            var values = token.Split(' ');
            return values.Select(p => p?.Replace("%20", " ").Replace("%25", "%")).ToArray();
        }


        public static string ToValidFileNameEnding(this string str)
        {
            var isNonAccepted = true;

            while (isNonAccepted)
            {
                var lastChar = str[str.Length - 1];

                isNonAccepted = lastChar == ' ' || lastChar == '.' || lastChar == ';' || lastChar == ':';

                if (isNonAccepted) str = str.Remove(str.Length - 1);
                else break;

                if (str.Length == 0) return str;

                isNonAccepted = lastChar == ' ' || lastChar == '.' || lastChar == ';' || lastChar == ':';
            }

            return str;
        }


        public static string ToHtmlStrippedText(this string str)
        {
            var array = new char[str.Length];
            var arrayIndex = 0;
            var inside = false;

            foreach (var o in str.ToCharArray())
            {
                switch (o)
                {
                    case '<':
                        inside = true;
                        continue;
                    case '>':
                        inside = false;
                        continue;
                }
                if (inside) continue;

                array[arrayIndex] = o;
                arrayIndex++;
            }
            return new string(array, 0, arrayIndex);
        }



        public static string Append(this string left, string right) => left + "-" + right;

        public static Uri ToUri(this string url, UriKind kind = UriKind.Absolute)
        {
            Uri myuri = new Uri(url, kind);
            return myuri;
        }

        public static string StripHtmlTags(this string str)
        {
            return HtmlRemoval.StripTagsRegex(str);
        }

        public static T TryDeserializeJson<T>(this string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return default(T);
            }
        }

        public static object TryDeserializeJsonWithTypeInfo(this string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            try
            {
                return JsonConvert.DeserializeObject(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
                });
            }
            catch
            {
                return null;
            }
        }

        public static string SerializeToJsonWithTypeInfo(this object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
                });
            }
            catch
            {
                return null;
            }
        }

        public static string SerializeToJson(this object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch
            {
                return null;
            }
        }




    }
}