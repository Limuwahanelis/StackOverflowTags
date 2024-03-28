using Azure;
using SOTags.Data;
using SOTags.Interfaces;
using SOTags.Model;
using SOTags.Repositories;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SOTags.Services
{
    public class StackExchangeService
    {
        private readonly ITagsRepository _tagsRepository;

        public StackExchangeService(ITagsRepository tagsRepository)
        {
            _tagsRepository = tagsRepository;
        }

        public async Task ImportTagsFromStackOverflow()
        {
            //if (true) return;
            int pageSize = 100;
            int pageIndex = 1;
            long totalNumberOfTagsUse = 0;
            string data = "";
            for (int numberOfEntries = 0; numberOfEntries < 1000;)
            {
                data = await GetPagedDataFromStackExchange(pageSize, pageIndex);
                List<Tag> tags = GetTagsFromStackExchangeResponse(data, ref totalNumberOfTagsUse);
                _tagsRepository.AddOrUpdateTagsToDatabase(tags);
                numberOfEntries += tags.Count;
                if (!JsonDocument.Parse(data).RootElement.GetProperty("has_more").GetBoolean()) break;
                pageIndex++;
            }
            Console.WriteLine($"total tags usage = {totalNumberOfTagsUse}");
            _tagsRepository.CalculateTagsUsage(totalNumberOfTagsUse);
        }

        public async Task UpdateTagsInDB(SOTagsDBContext context)
        {
            int pageSize = 100;
            int pageIndex = 1;
            long totalNumberOfTagsUse = 0;
            List<Tag> tags;

            for (int numberOfTagsUpdated = 0; numberOfTagsUpdated < _tagsRepository.GetNumberOfTagsInDB();)
            {
                List<string?> tagsNames = _tagsRepository.GetTagsName(1 + (pageIndex - 1) * pageSize, pageIndex * pageSize);
                string tagsToUpdate = string.Join(";", tagsNames);

                // If tags contains some special characters it needs to be properly encoded for request to work
                string tagsUrl = HttpUtility.UrlEncode(tagsToUpdate);
                string data = await GetTagsInfoFromStackExchange(pageSize, tagsUrl);

                tags = GetTagsFromStackExchangeResponse(data, ref totalNumberOfTagsUse);
                _tagsRepository.AddOrUpdateTagsToDatabase(tags);
                numberOfTagsUpdated += tagsNames.Count;
                pageIndex++;
            }
            _tagsRepository.CalculateTagsUsage(totalNumberOfTagsUse);
        }
        private async Task<string> GetTagsInfoFromStackExchange(int pageSize, string tagsUrl)
        {
            string path = $"https://api.stackexchange.com/2.3/tags/{tagsUrl}/info?pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8";
            string data="";
            using (HttpResponseMessage response = await APIHelper.client.GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    data = await response.Content.ReadAsStringAsync();
                }
                //var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //data = System.IO.File.ReadAllText(absPath + "/Jsons/data.json");
            }
            return data;
        }
        private async Task<string> GetPagedDataFromStackExchange(int pageSize, int pageIndex)
        {
            string path = $"https://api.stackexchange.com/2.3/tags?page={pageIndex}&pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8";
            string data = "";
            using (HttpResponseMessage response = await APIHelper.client.GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    data = await response.Content.ReadAsStringAsync();
                }
                //var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //data = System.IO.File.ReadAllText(absPath + "/Jsons/data.json");
            }
            return data;
        }
        private List<Tag> GetTagsFromStackExchangeResponse(string data,ref long totalNumberOfTagsUse)
        {
            List<Tag> tags = new List<Tag>();
            JsonDocument jsonDocument = JsonDocument.Parse(data);
            JsonElement tagsList = jsonDocument.RootElement.GetProperty("items");
            foreach (JsonElement element in tagsList.EnumerateArray())
            {
                Tag tag = element.Deserialize<Tag>();
                tags.Add(tag);
                totalNumberOfTagsUse += tag.Count;
            }
            return tags;
        }
    }
}
