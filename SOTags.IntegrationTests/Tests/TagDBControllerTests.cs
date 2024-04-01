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
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SOTags.IntegrationTests.Tests
{
    public class TagDBControllerTests
    {
        [Fact]
        public async void GetTags_ReturnsTags()
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
        public async void dd()
        {
            WireMockServer server;
            HttpClient client;
            server = WireMockServer.Start(5555);
            var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string responseData = File.ReadAllText(absPath + $"/Jsons/data{1}.json");
            server.Given(Request.Create()
                .UsingGet()
                .WithPath("/tags")
                .WithParam("pageSize", "200")
                .WithParam("pageNumber", "1")
                ).RespondWith(Response.Create()
                .WithStatusCode(System.Net.HttpStatusCode.OK)
                .WithBody(responseData));
            client = server.CreateClient();

            var response = await client.GetAsync("/tags?pageNumber=1&pageSize=200");
            string data = await response.Content.ReadAsStringAsync();


        }
        //[Fact]
        //public async void GetTags_ImportTags()
        //{
        //    //_server = WireMockServer.Start();
        //    //_server.gi
        //    var application = new SOTagsWebApplicationFactory();

        //    var client = application.CreateClient();
        //    var response = await client.GetAsync("/TagDB/Update");
        //    string content = await response.Content.ReadAsStringAsync();
        //    response.EnsureSuccessStatusCode();
        //    Assert.True(response.IsSuccessStatusCode);
        //}



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
