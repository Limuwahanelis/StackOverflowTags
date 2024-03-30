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
            TagParemeters tagParemeters = new TagParemeters();
            var response = await client.GetAsync("/TagDB?pageNumber=1&pageSize=200");
            string aa = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
          //Assert.NotNull(response);
        }
    }
}
