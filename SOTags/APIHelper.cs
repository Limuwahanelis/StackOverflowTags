using SOTags.Model;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SOTags
{
    public class APIHelper
    {

        private static HttpClientHandler handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        public static HttpClient client = new HttpClient(handler);

        public static void SetUP()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
