using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SOTags.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace SOTags.IntegrationTests
{
    internal class SOTagsWebApplicationFactory: WebApplicationFactory<Program>
    {
        private const string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=EFTestSample;Trusted_Connection=True";
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<SOTagsDBContext>));
                services.AddSqlServer<SOTagsDBContext>(ConnectionString);
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
