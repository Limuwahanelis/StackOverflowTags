using SOTags.Data;
using SOTags.Model;
using System.Diagnostics;
using System.Text.Json;

namespace SOTags.Services
{
    public class StackExchangeService
    {
        //https://api.stackexchange.com/2.3/tags?page=1&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8
        public async Task ImportTagsToDB(string path,SOTagsDBContext context)
        {
            string data = "";
            using (HttpResponseMessage response = await APIHelper.client.GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    data = await response.Content.ReadAsStringAsync();

                }
                using (context)
                {
                    context.Database.EnsureCreated();

                    List<Tag> tags = new List<Tag>();
                    JsonDocument jsonDocument = JsonDocument.Parse(data);
                    JsonElement tagsList = jsonDocument.RootElement.GetProperty("items");
                    long totalTagUse = 0;
                    foreach (JsonElement element in tagsList.EnumerateArray())
                    {
                        Tag tag = element.Deserialize<Tag>();
                        tags.Add(tag);
                        totalTagUse += tag.Count;
                    }
                    foreach(Tag tag in tags)
                    {
                        tag.UsePercentage = float.Round(100f * tag.Count / totalTagUse, 2);
                        Console.WriteLine(tag.UsePercentage);
                        context.Tags.Add(tag);
                    }
                    //Console.WriteLine(context.Tags.First().UsePercentage);
                    context.SaveChanges();
                }
            }
        }
    }
}
