using SOTags.CustomDataFormats;
using SOTags.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SOTags.IntegrationTests
{
    public class TagDBControllerTests
    {
        [Fact]
        public async void GetTags_ReturnsTags()
        {
            var application = new SOTagsWebApplicationFactory();

            var client = application.CreateClient();
            var response = await client.GetAsync("/TagDB?pageNumber=1&pageSize=200");
            string data = await response.Content.ReadAsStringAsync();
            List<Tag> tags = GetTagsFromResponse(data);
            response.EnsureSuccessStatusCode();
            Assert.NotNull(tags);
        }

        [Fact]
        public async void GetTags_ImportTags()
        {
            var application = new SOTagsWebApplicationFactory();

            var client = application.CreateClient();
            var response = await client.GetAsync("/TagDB/Update");
            string content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Assert.True(response.IsSuccessStatusCode);
        }



        private List<Tag> GetTagsFromResponse(string data)
        {
            List<Tag> tags = new List<Tag>();
            JsonDocument jsonDocument = JsonDocument.Parse(data);
            JsonElement tagsList = jsonDocument.RootElement;
            foreach (JsonElement element in tagsList.EnumerateArray())
            {
                Tag tag = element.Deserialize<Tag>();
                tags.Add(tag);
            }
            return tags;
        }
    }
}
