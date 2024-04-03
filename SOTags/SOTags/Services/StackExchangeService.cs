
using SOTags.Exceptions;
using SOTags.Interfaces;
using System.Net.Http.Headers;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace SOTags.Services
{
    public class StackExchangeService : IStackExchangeService
    {
        readonly string filter = "";

        //private readonly IHttpClientFactory _httpClientFactory;
        HttpClient client;
        private readonly IConfiguration _configuration;

        public StackExchangeService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            filter = _configuration.GetValue<string>("StackExchangeServer:Filter");
            client = httpClient;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetTagsInfoFromStackExchange(int pageSize, string tagsUrl)
        {
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(
            //new MediaTypeWithQualityHeaderValue("application/json"));
            string path = _configuration.GetValue<string>("StackExchangeServer:Address") + $"tags/{tagsUrl}/info?pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter={filter}";
            string data = "";
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new StackExchangeServerCouldNotBeReachedException()
                {
                    StackExchangeSetverMessage = await response.Content.ReadAsStringAsync()
                };
            }
            return data;
        }
        public async Task<string> GetPagedDataFromStackExchange(int pageSize, int pageIndex)
        {

            string path = _configuration.GetValue<string>("StackExchangeServer:Address") + $"tags?page={pageIndex}&pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter={filter}";
            string data = "";
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new StackExchangeServerCouldNotBeReachedException()
                {
                    StackExchangeSetverMessage = await response.Content.ReadAsStringAsync()
                };
            }
            return data;
        }

    }
}
