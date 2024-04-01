using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SOTags.Data;
using SOTags.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
namespace SOTags.IntegrationTests.Architecture
{
    internal class SOTagsWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly IHttpClientFactory? _fakeApiHttpClientFactory;
        private const string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=IntegrationTestsDB;Trusted_Connection=True";

        public SOTagsWebApplicationFactory(IHttpClientFactory? fakeApiHttpClientFactory)
        {
            _fakeApiHttpClientFactory = fakeApiHttpClientFactory;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.UseEnvironment("IntegrationTests");
            builder.ConfigureTestServices(services =>
            {

                services.RemoveAll(typeof(DbContextOptions<SOTagsDBContext>));
                services.AddSqlServer<SOTagsDBContext>(ConnectionString);
                if (_fakeApiHttpClientFactory is not null)
                {
                    services.AddSingleton(_fakeApiHttpClientFactory);
                }
                var dbContext = CreateDBContext(services);
                dbContext.Database.EnsureDeleted();
            });
        }

        private static SOTagsDBContext CreateDBContext(IServiceCollection services)
        {
            var serviceProvide = services.BuildServiceProvider();
            var scope = serviceProvide.CreateScope();
            SOTagsDBContext dBContext = scope.ServiceProvider.GetRequiredService<SOTagsDBContext>();
            return dBContext;

        }
    }
}
