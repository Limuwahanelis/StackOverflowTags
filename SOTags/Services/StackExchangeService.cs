using Azure;
using SOTags.Data;
using SOTags.Model;
using SOTags.Repositories;
using System.Diagnostics;
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
        private readonly TagsRepository _tagsRepository;

        public StackExchangeService(TagsRepository tagsRepository)
        {
            _tagsRepository = tagsRepository;
        }

        public async Task ImportTagsFromStackOverflow()
        {
            int pageSize = 100;
            int pageIndex = 1;
            long totalNumberOfTagsUse = 0;
            string data = "";
            //for (int numberOfEntries = 0; numberOfEntries < 10;)
            //{
                string path = $"https://api.stackexchange.com/2.3/tags?page={pageIndex}&pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8";

                using (HttpResponseMessage response = await APIHelper.client.GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        //data = await response.Content.ReadAsStringAsync();
                    }
                    var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    data = System.IO.File.ReadAllText(absPath + "/Jsons/data.json");
                    List<Tag> tags = GetTagsFromStackExchange(data, ref totalNumberOfTagsUse);
                    _tagsRepository.InitalizeDatabase(tags);
                    //numberOfEntries += tags.Count;
                    //if (!JsonDocument.Parse(data).RootElement.GetProperty("has_more").GetBoolean()) break;
                    pageIndex++;

                }
            //}
            Console.WriteLine($"total tags usage = {totalNumberOfTagsUse}");
            _tagsRepository.CalculateTagsUsage(totalNumberOfTagsUse);
        }
        public async Task UpdateTagsInDB(SOTagsDBContext context)
        {
            int pageSize = 100;
            int pageIndex = 1;
            long totalNumberOfTagsUse = 0;
            string path;
            string data = "";
            List<Tag> tags;
            using (context)
            {
                for (int numberOfEntries = 0; numberOfEntries < 1000;)
                {
                    string tagsToUpdate = string.Join(";", context.Tags.Where(tag => tag.Id <= pageSize * pageIndex).Select(tag => tag.Name).ToList());
                    // If tags contains some special characters it needs to be properly encode for request to work
                    string tagsUrl = HttpUtility.UrlEncode(tagsToUpdate);
                    path = $"https://api.stackexchange.com/2.3/tags/{tagsUrl}/info?pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8";
                    using (HttpResponseMessage response = await APIHelper.client.GetAsync(path))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            data = await response.Content.ReadAsStringAsync();
                        }
                        tags = GetTagsFromStackExchange(data, ref totalNumberOfTagsUse);

                        List<Tag> toUpdate = context.Tags.Where(t => t.Id <= 10).ToList();
                        for (int i = 0; i < 10; i++)
                        {
                            toUpdate[i].Name = tags[i].Name;
                            toUpdate[i].Count = tags[i].Count;
                        }
                        numberOfEntries += tags.Count;
                        pageIndex++;

                    }
                }
                Console.WriteLine($"total tags usage = {totalNumberOfTagsUse}");
                foreach (var tag in context.Tags)
                {
                    tag.UsePercentage = float.Round(100f * tag.Count / totalNumberOfTagsUse, 2);
                }

                context.SaveChanges();
            }
        }

        private List<Tag> GetTagsFromStackExchange(string data,ref long totalNumberOfTagsUse)
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
