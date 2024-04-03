using Humanizer;
using Microsoft.CodeAnalysis.Text;
using SharpYaml;
using SOTags.CustomDataFormats;
using SOTags.IntegrationTests.Architecture;
using SOTags.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using static Google.Protobuf.Reflection.FieldDescriptorProto;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SOTags.IntegrationTests.Tests
{
    public class TagDBControllerTests
    {
        [Fact]
        public async void GetTags_ReturnsTags_AssertNotNull()
        {
            TestHttpClientFactory httpFactory = new TestHttpClientFactory();
            var application = new SOTagsWebApplicationFactory(httpFactory);
            var client = application.CreateClient();
            var response = await client.GetAsync("/TagDB?pageNumber=1&pageSize=10");
            string data = await response.Content.ReadAsStringAsync();
            List<Tag> tags = GetTagsFromResponse(data);
            response.EnsureSuccessStatusCode();
            Assert.NotNull(tags);
            httpFactory.Dispose();
        }
        [Fact]
        public async void GetTags_ImportTags_AssertSuccessfulResponse()
        {
            TestHttpClientFactory httpFactory = new TestHttpClientFactory();
            var application = new SOTagsWebApplicationFactory(httpFactory);

            var client = application.CreateClient();
            var response = await client.GetAsync("/TagDB/Import");
            string content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Assert.True(response.IsSuccessStatusCode);

            httpFactory.Dispose();
        }
        [Fact]
        public async void GetTags_ImportTags_AssertUnsucessfulResponseAndMessageContent()
        {
            UnsucessfulResponsClientFactory httpFactory = new UnsucessfulResponsClientFactory();
            var application = new SOTagsWebApplicationFactory(httpFactory);

            var client = application.CreateClient();
            var response = await client.GetAsync("/TagDB/Import");
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal("An problem occured when reaching Stack Exchange server. Message from server:"+
            " \nManaged to Imported or updated 40 tags from required 100 entries", content);
            Assert.True(!response.IsSuccessStatusCode);

            httpFactory.Dispose();
        }
        
        private List<Tag> GetTagsFromResponse(string data)
        {
            List<Tag> tags = new List<Tag>();
            JsonDocument jsonDocument = JsonDocument.Parse(data);
            JsonElement tagsList = jsonDocument.RootElement;
            foreach (JsonElement element in tagsList.EnumerateArray())
            {
                Tag? tag = element.Deserialize<Tag>();
                if(tag!=null) tags.Add(tag);
            }
            return tags;
        }
    }
}
