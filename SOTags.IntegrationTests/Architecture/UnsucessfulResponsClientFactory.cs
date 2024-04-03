using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SOTags.IntegrationTests.Architecture
{
    public class UnsucessfulResponsClientFactory: IHttpClientFactory, IDisposable
    {
        static WireMockServer server;
        static List<HttpClient> clients = new List<HttpClient>();
        static string filter = "!T.BkwE7kN-8V1(Ty1E";
        private readonly Lazy<HttpMessageHandler> _handlerLazy = new(() => new HttpClientHandler());
        static UnsucessfulResponsClientFactory()
        {
            server = WireMockServer.Start();
            var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            for (int i = 0; i < 10; i++)
            {
                string data = File.ReadAllText(absPath + $"/Jsons/data{i + 1}.json");
                if (i == 3)
                {
                    server.Given(Request.Create()
                        .UsingGet()
                        .WithPath("/tags")
                        .WithParam("pagesize", "10")
                       .WithParam("page", (i + 1).ToString())
                        .WithParam("order", "desc")
                        .WithParam("sort", "popular")
                        .WithParam("site", "stackoverflow")
                        .WithParam("filter", filter))
                    .InScenario("Initialization")
                    .WillSetStateTo("Send bad response")
                    .RespondWith(Response.Create()
                    .WithStatusCode(System.Net.HttpStatusCode.OK)
                    .WithBody(data));
                    continue;
                }
                if (i == 4)
                {
                    server.Given(Request.Create()
                        .UsingGet()
                        .WithPath("/tags")
                        .WithParam("pagesize", "10")
                       .WithParam("page", (i + 1).ToString())
                        .WithParam("order", "desc")
                        .WithParam("sort", "popular")
                        .WithParam("site", "stackoverflow")
                        .WithParam("filter", filter))
                    .InScenario("Initialization")
                    .WhenStateIs("Send bad response")
                    .RespondWith(Response.Create()
                    .WithStatusCode(System.Net.HttpStatusCode.BadRequest));
                }
                server.Given(Request.Create()
                    .UsingGet()
                    .WithPath("/tags")
                    .WithParam("pagesize", "10")
                    .WithParam("page", (i + 1).ToString())
                    .WithParam("order", "desc")
                    .WithParam("sort", "popular")
                    .WithParam("site", "stackoverflow")
                    .WithParam("filter", filter))
                 .InScenario("Initialization")
                 .RespondWith(Response.Create()
                 .WithStatusCode(System.Net.HttpStatusCode.OK)
                 .WithBody(data));
            }
        }
        public HttpClient CreateClient(string name)
        {
            HttpClient client = server.CreateClient();
            clients.Add(client);
            return client;
        }
        public void Dispose()
        {
            if (_handlerLazy.IsValueCreated)
            {
                _handlerLazy.Value.Dispose();
            }
            server.Dispose();
            foreach (var client in clients)
            {
                client.Dispose();
            }
        }

    }
}
