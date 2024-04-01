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

namespace SOTags.IntegrationTests.Architecture
{
    public class TestHttpClientFactory : IHttpClientFactory, IDisposable
    {
        static WireMockServer server;
        static HttpClient client;
        static string filter = "!T.BkwE7kN-8V1(Ty1E";
        private readonly Lazy<HttpMessageHandler> _handlerLazy = new(() => new HttpClientHandler());
        static TestHttpClientFactory()
        {

            var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            server = WireMockServer.Start();
            for (int i = 0; i < 10; i++)
            {
                string requestPath = $"tags?page={i + 1}&pagesize=10&order=desc&sort=popular&site=stackoverflow&filter={filter}";
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
            client = server.CreateClient();
        }
        public HttpClient CreateClient(string name)
        {

            return client;
        }

        public void Dispose()
        {
            if (_handlerLazy.IsValueCreated)
            {
                _handlerLazy.Value.Dispose();
            }
            client.Dispose();
            server.Dispose();
        }

    }
}
