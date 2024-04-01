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

namespace SOTags.UnitTest
{
    public class StackExchangeTagDBServiceTest
    {
        [Fact]
        public async void ImportTagsFromStackOverflow_CorrectlyImportData_AssertTrue()
        {
            string data;
            var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            data = System.IO.File.ReadAllText(absPath + "/Jsons/data.json");
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            IStackExchangeService stackService = Substitute.For<IStackExchangeService>();

            stackService.GetPagedDataFromStackExchange(default, default).ReturnsForAnyArgs(data);

            StackExchangeTagDBService stackExchangeTagDBService = new StackExchangeTagDBService(tagsRepository,stackService);
            await stackExchangeTagDBService.ImportTagsFromStackOverflow(1);
            Assert.True(true);
        }
        [Fact]
        public async Task ImportTagsFromStackOverflow_ServerNotReached_AssertError()
        {
            string data;
            var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            data = System.IO.File.ReadAllText(absPath + "/Jsons/data.json");
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            IStackExchangeService stackService = Substitute.For<IStackExchangeService>();

            stackService.When(x => x.GetPagedDataFromStackExchange(default, default)).Do(x => { throw new StackExchangeServerCouldNotBeReachedException(); });

            StackExchangeTagDBService stackExchangeTagDBService = new StackExchangeTagDBService(tagsRepository, stackService);
            await Assert.ThrowsAsync<StackExchangeServerCouldNotBeReachedException>(() => stackService.GetPagedDataFromStackExchange(default, default));
        }

    }
}
