using Azure;
using SOTags.Data;
using SOTags.Model;
using System.Diagnostics;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SOTags.Services
{
    public class StackExchangeService
    {
        //https://api.stackexchange.com/2.3/tags?page=1&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8
        public async Task ImportTagsToDB(string path,SOTagsDBContext context)
        {
            int pageSize=100;
            int pageIndex=1;
            long totalNumberOfTagsUse = 0;
            string data = "";
            using (context)
            {
                for (int numberOfEntries = 0; numberOfEntries < 1000;)
                {
                    path = $"https://api.stackexchange.com/2.3/tags?page={pageIndex}&pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8";
                   
                    using (HttpResponseMessage response = await APIHelper.client.GetAsync(path))
                    {

                        if (response.IsSuccessStatusCode)
                        {
                            data = await response.Content.ReadAsStringAsync();
                        }

                        List<Tag> tags = GetTagsFromStackExchange(data, ref totalNumberOfTagsUse);
                        foreach (Tag tag in tags)
                        {
                            //tag.UsePercentage = float.Round(100f * tag.Count / totalNumberOfTagsUse, 2);
                            
                            context.Tags.Add(tag);
                        }
                        numberOfEntries += tags.Count;
                        //context.SaveChanges();

                        if (!JsonDocument.Parse(data).RootElement.GetProperty("has_more").GetBoolean()) break;
                        pageIndex++;

                    }
                }
                context.SaveChanges();
                Console.WriteLine($"total tags usage = { totalNumberOfTagsUse}");
                foreach (var tag in context.Tags)
                {
                    tag.UsePercentage = float.Round(100f * tag.Count / totalNumberOfTagsUse, 2);
                    Console.WriteLine(tag.UsePercentage);
                }
                context.SaveChanges();
            }
        }
        //public async Task UpdateTagsInDB(string path, SOTagsDBContext context)
        //{
        //    using (HttpResponseMessage response = await APIHelper.client.GetAsync(path))
        //    {
        //        string data = "";
        //        if (response.IsSuccessStatusCode)
        //        {
        //            data = await response.Content.ReadAsStringAsync();
        //        }
        //        using (context)
        //        {
        //            List<Tag> tags = GetTagsFromStackExchange(data, out long totalNumberOfTagsUse);
        //            foreach (Tag tag in tags)
        //            {
        //                tag.UsePercentage = float.Round(100f * tag.Count / totalNumberOfTagsUse, 2);
        //                Console.WriteLine(tag.UsePercentage);
        //                context.Tags.
        //            }
        //            //Console.WriteLine(context.Tags.First().UsePercentage);
        //            context.SaveChanges();
        //        }
        //    }
        //}

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
