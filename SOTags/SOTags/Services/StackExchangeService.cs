
using Microsoft.IdentityModel.Tokens;
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

        private readonly IConfiguration _configuration;
        private readonly HttpClient client;

        public StackExchangeService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            string? configFilter = _configuration.GetValue<string>("StackExchangeServer:Filter");
            filter = configFilter!=null?configFilter:"";

            client = httpClient;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<string> GetPagedDataFromStackExchange(int pageSize, int pageIndex)
        {
            string path = _configuration.GetValue<string>("StackExchangeServer:Address") + $"tags?page={pageIndex}&pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter={filter}";
            HttpResponseMessage response = await client.GetAsync(path);
            string data;
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
