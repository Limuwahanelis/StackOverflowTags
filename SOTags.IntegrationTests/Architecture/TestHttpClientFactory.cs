using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using System.Reflection;
using System.Text.Json;
using SOTags.Model;
using System.Web;

namespace SOTags.IntegrationTests.Architecture
{
    public class TestHttpClientFactory : IHttpClientFactory, IDisposable
    {
        static List<WireMockServer> servers= new List<WireMockServer>();
        static List<HttpClient> clients = new List<HttpClient>();
        static string filter = "!T.BkwE7kN-8V1(Ty1E";
        private readonly Lazy<HttpMessageHandler> _handlerLazy = new(() => new HttpClientHandler());
        public HttpClient CreateClient(string name)
        {

            var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            WireMockServer server = WireMockServer.Start();
            for (int i = 0; i < 10; i++)
            {
                //string requestPath = $"tags/{tagsUrl}/info?pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter={filter}";
                string data = File.ReadAllText(absPath + $"/Jsons/data{i + 1}.json");
                server.Given(Request.Create()
                .UsingGet()
                .WithPath("/tags")
                .WithParam("pagesize", "10")
                .WithParam("page", (i + 1).ToString())//path => path.Contains("tags?"))
                .WithParam("order", "desc")
                .WithParam("sort", "popular")
                .WithParam("site", "stackoverflow")
                .WithParam("filter", filter)
                ).RespondWith(Response.Create()
                .WithStatusCode(System.Net.HttpStatusCode.OK)
                .WithBody(data));
            }
            for (int i = 0; i < 10; i++)
            {
                //string requestPath = $"tags/{tagsUrl}/info?pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter={filter}";
                List<string> requestList = new List<string>();
                string tagsNames = File.ReadAllText(absPath + $"/Jsons/data{i + 1}.json");
                string data = File.ReadAllText(absPath + $"/Jsons/data{i + 1}Update.json");
                requestList = GetTagsNamesFromData(tagsNames);
                string tagsToUpdate = string.Join(";", requestList);
                string tagsUrl = HttpUtility.UrlEncode(tagsToUpdate);
                server.Given(Request.Create()
                .UsingGet()
                .WithPath("/tags/" + tagsToUpdate + "/info")
                .WithParam("pagesize", "10")
                .WithParam("order", "desc")
                .WithParam("sort", "popular")
                .WithParam("site", "stackoverflow")
                .WithParam("filter", filter)
                ).RespondWith(Response.Create()
                .WithStatusCode(System.Net.HttpStatusCode.OK)
                .WithBody(data));
            }
            HttpClient client = server.CreateClient();
            servers.Add(server);
            clients.Add(client);
            return client;
        }
        static private List<string> GetTagsNamesFromData(string data)
        {
            List<string> tagsNames = new List<string>();
            List<Tag> tags = new List<Tag>();
            JsonDocument jsonDocument = JsonDocument.Parse(data);
            JsonElement tagsList = jsonDocument.RootElement.GetProperty("items");
            foreach (JsonElement element in tagsList.EnumerateArray())
            {
                Tag tag = element.Deserialize<Tag>();
                tagsNames.Add(tag.Name);
            }
            return tagsNames;
        }
        public void Dispose()
        {
            if (_handlerLazy.IsValueCreated)
            {
                _handlerLazy.Value.Dispose();
            }
            foreach (var server in servers)
            {
                server.Dispose();
            }
            foreach(var client in clients)
            {
                client.Dispose();
            }
        }

    }
}
