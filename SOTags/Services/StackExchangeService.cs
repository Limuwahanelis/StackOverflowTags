
using SOTags.Exceptions;
using SOTags.Interfaces;

namespace SOTags.Services
{
    public class StackExchangeService: IStackExchangeService
    {
        public async Task<string> GetTagsInfoFromStackExchange(int pageSize, string tagsUrl)
        {
            string path = $"https://api.stackexchange.com/2.3/tags/{tagsUrl}/info?pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8";
            string data="";
            using (HttpResponseMessage response = await APIHelper.client.GetAsync(path))
            {
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
                //var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //data = System.IO.File.ReadAllText(absPath + "/Jsons/data.json");
            }
            return data;
        }
        public async Task<string> GetPagedDataFromStackExchange(int pageSize, int pageIndex)
        {
            string path = $"https://api.stackexchange.com/2.3/tags?page={pageIndex}&pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8";
            string data = "";
            using (HttpResponseMessage response = await APIHelper.client.GetAsync(path))
            {
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
                //var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //data = System.IO.File.ReadAllText(absPath + "/Jsons/data.json");
            }
            return data;
        }

    }
}
