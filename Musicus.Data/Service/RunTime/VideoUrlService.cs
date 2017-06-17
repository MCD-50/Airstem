using Musicus.Data.Service.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Musicus.Data.Service.RunTime
{
    public class VideoUrlService : IVideoUrlService
    {
        public async Task<string> GetUrlAsync(string id, bool isHd)
        {
            string uri = null;

            string json = await Post(id);
            JObject _object = JObject.Parse(json);
            JArray _array = (JArray)_object["message"];
           
            for(int i = _array.Count - 1; i >= 0; i--)
            {
                var x = _array[i];
                if (isHd)
                {
                    var height = int.Parse(x["height"].ToString());
                    if (height > 500)
                    {
                        uri = x["url"].ToString();
                        break;
                    }
                }
                else
                {
                    var height = int.Parse(x["height"].ToString());
                    if(height <= 500)
                    {
                        uri = x["url"].ToString();
                        break;

                    }

                }
            }

            return uri;
        }


        private async Task<string> Post(string id)
        {
            StringBuilder sb = new StringBuilder();


            WebRequest request = WebRequest.Create("https://youtube-fetcher.herokuapp.com/video/id=" + id);
            request.Method = "GET";
            request.Headers["Accept"] = "application/json";
            request.Headers["ContentType"] = "application/json";


            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        while (sr.Peek() >= 0)
                        {
                            sb.Append(sr.ReadLine());
                        }

                    }
                }
            }

            return sb.ToString();
        }


    }
}
