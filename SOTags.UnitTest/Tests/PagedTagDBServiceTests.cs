using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SOTags.CustomDataFormats;
using SOTags.Interfaces;
using SOTags.Model;
using SOTags.Repositories;
using SOTags.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SOTags.UnitTest.Tests
{

    public class PagedTagDBServiceTests
    {

        [Fact]
        public void GetTags_PageIsSize3DataIsBigger_RetrunsTrue()
        {
            string data;
            var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            data = File.ReadAllText(absPath + "/Jsons/data.json");
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            var testData = new List<Tag>()
            {
                new Tag {Name ="c#",Count=2,UsePercentage =50f},
                new Tag {Name ="c++",Count=5, UsePercentage= 2f},
                new Tag {Name ="java",Count=5,UsePercentage= 1f},
                new Tag {Name ="dd",Count=5,UsePercentage= 60f},
                new Tag {Name ="ff",Count=5,UsePercentage= 60f}
            };
            PagedTagDBService pagedTagDBService = new PagedTagDBService(tagsRepository);
            TagParemeters par = new TagParemeters()
            {
                PageSize = 3,
            };
            tagsRepository.GetTagsPaged(par.PageSize, par.PageNumber, TagSortingHelper.TagSortingType.ID, par.IsDescending, out Arg.Any<int>()).Returns(x => { x[4] = 10; return testData; });
            PagedList<Tag> tags = pagedTagDBService.GetTags(par);
            Assert.True(tags.HasNext);
        }
        [Fact]
        public void GetTags_SortedByNameAscending_RetrunsAbs()
        {
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            var testData = new List<Tag>()
            {
                new Tag {Name ="abs",Count=2,UsePercentage =50f},
                new Tag {Name ="c#",Count=2,UsePercentage =50f},
                new Tag {Name ="c++",Count=5, UsePercentage= 2f},
                new Tag {Name ="dd",Count=5,UsePercentage= 60f},
                new Tag {Name ="java",Count=5,UsePercentage= 1f}
            };
            //tagsRepository.GetAllTags().Returns(testData);
            PagedTagDBService pagedTagDBService = new PagedTagDBService(tagsRepository);
            TagParemeters par = new TagParemeters()
            {
                SortBy = "Name",
                IsDescending = false,
            };
            tagsRepository.GetTagsPaged(par.PageSize, par.PageNumber, TagSortingHelper.TagSortingType.NAME, par.IsDescending, out Arg.Any<int>()).Returns(x => { x[4] = 10; return testData; });
            PagedList<Tag> tags = pagedTagDBService.GetTags(par);
            Assert.Equal("abs", tags[0].Name);
        }
        [Fact]
        public void GetTags_DataIsBiggerThanPagePageIs3_RetrunHasPreviousTrue()
        {
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            var testData = new List<Tag>()
            {
                new Tag {Name ="c#",Count=2,UsePercentage =50f},
                new Tag {Name ="c++",Count=5, UsePercentage= 2f},
                new Tag {Name ="abs",Count=2,UsePercentage =50f},
                new Tag {Name ="java",Count=5,UsePercentage= 1f},
                new Tag {Name ="dd",Count=5,UsePercentage= 60f}
            };
            //tagsRepository.GetAllTags().Returns(testData);
            PagedTagDBService pagedTagDBService = new PagedTagDBService(tagsRepository);
            TagParemeters par = new TagParemeters()
            {
                SortBy = "Name",
                IsDescending = false,
                PageNumber = 2,
            };
            tagsRepository.GetTagsPaged(par.PageSize, par.PageNumber, TagSortingHelper.TagSortingType.NAME, par.IsDescending, out Arg.Any<int>()).Returns(x => { x[4] = 10; return testData; });
            PagedList<Tag> tags = pagedTagDBService.GetTags(par);
            Assert.True(tags.HasPrevious);
        }

    }
}
