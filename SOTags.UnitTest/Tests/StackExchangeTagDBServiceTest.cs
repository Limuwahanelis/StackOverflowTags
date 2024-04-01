using Castle.Components.DictionaryAdapter;
using NSubstitute;
using NSubstitute.Extensions;
using SOTags.Interfaces;
using SOTags.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SOTags.Exceptions;
using Microsoft.Extensions.Configuration;

namespace SOTags.UnitTest.Tests
{

    public class StackExchangeTagDBServiceTest
    {
        Dictionary<string, string?> intSettings = new Dictionary<string, string?>
            {
                {"StackExchangeServer:PageSize","100" },
                {"StackExchangeServer:NumberOfRequiredTags","1000" }
            };
        string dataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [Fact]
        public async void ImportTagsFromStackOverflow_CorrectlyImportData_AssertTrue()
        {
            SetUp(out string data);
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            IStackExchangeService stackService = Substitute.For<IStackExchangeService>();
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(intSettings).Build();
            stackService.GetPagedDataFromStackExchange(default, default).ReturnsForAnyArgs(data);

            StackExchangeTagDBService stackExchangeTagDBService = new StackExchangeTagDBService(tagsRepository, stackService, config);
            await stackExchangeTagDBService.ImportTagsFromStackOverflow();

            Assert.True(true);
        }
        [Fact]
        public async Task ImportTagsFromStackOverflow_ServerNotReached_AssertError()
        {
            SetUp(out string data);
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            IStackExchangeService stackService = Substitute.For<IStackExchangeService>();
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(intSettings).Build();

            stackService.GetPagedDataFromStackExchange(default, default).ReturnsForAnyArgs(data);
            stackService.When(x => x.GetPagedDataFromStackExchange(default, default)).Do(x => { throw new StackExchangeServerCouldNotBeReachedException(); });

            StackExchangeTagDBService stackExchangeTagDBService = new StackExchangeTagDBService(tagsRepository, stackService, config);

            await Assert.ThrowsAsync<StackExchangeServerCouldNotBeReachedException>(() => stackService.GetPagedDataFromStackExchange(default, default));
        }

        private void SetUp(out string data)
        {
            data = File.ReadAllText(dataPath + "/Jsons/data.json");
        }
    }
}
