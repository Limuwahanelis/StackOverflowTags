using SOTags.Data;
using SOTags.Model;
using System.Text.Json;

namespace SOTags.Services
{
    public class StackExchangeService
    {
        //https://api.stackexchange.com/2.3/tags?page=1&order=desc&sort=popular&site=stackoverflow&filter=!bMsg5CXCu9jto8
        public async Task ImportTagsToDB(string path,SOTagsDBContext context)
        {
            List<Tag> tags = new List<Tag>();
            Tag? tag;
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


                    JsonDocument jsonDocument = JsonDocument.Parse(data);
                    JsonElement tagsList = jsonDocument.RootElement.GetProperty("items");
                    foreach (JsonElement element in tagsList.EnumerateArray())
                    {
                        context.Tags.Add(element.Deserialize<Tag>());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
