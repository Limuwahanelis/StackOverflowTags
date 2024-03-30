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

namespace SOTags.UnitTest
{
    
    public class PagedTagDBServiceTests
    {

        [Fact]
        public void GetTags_PageIsSize3DataIs5Size_RetrunsTrue()
        {
            string data;
            var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            data = System.IO.File.ReadAllText(absPath + "/Jsons/data.json");
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            var testData = new List<Tag>()
            {
                new Tag {Name ="c#",Count=2,UsePercentage =50f},
                new Tag {Name ="c++",Count=5, UsePercentage= 2f},
                new Tag {Name ="java",Count=5,UsePercentage= 1f},
                new Tag {Name ="dd",Count=5,UsePercentage= 60f}
            }.AsQueryable();
            tagsRepository.GetAllTags().Returns(testData);
            PagedTagDBService pagedTagDBService = new PagedTagDBService(tagsRepository);
            TagParemeters par = new TagParemeters()
            {
                PageSize = 3
            };
            PagedList<Tag> tags = pagedTagDBService.GetTags(par);
            Assert.Equal(3, tags.Count);
        }
        [Fact]
        public void GetTags_SortedByNameAscending_RetrunsTrue()
        {
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            var testData = new List<Tag>()
            {
                new Tag {Name ="c#",Count=2,UsePercentage =50f},
                new Tag {Name ="c++",Count=5, UsePercentage= 2f},
                new Tag {Name ="abs",Count=2,UsePercentage =50f},
                new Tag {Name ="java",Count=5,UsePercentage= 1f},
                new Tag {Name ="dd",Count=5,UsePercentage= 60f}
            }.AsQueryable();
            tagsRepository.GetAllTags().Returns(testData);
            PagedTagDBService pagedTagDBService = new PagedTagDBService(tagsRepository);
            TagParemeters par = new TagParemeters()
            {
                SortBy = "Name",
                IsDescending = false,
            };
            PagedList<Tag> tags = pagedTagDBService.GetTags(par);
            Assert.Equal("abs", tags[0].Name);
        }
        [Fact]
        public void GetTags_SortedByUsePercentageAscending_RetrunsTrue()
        {
            ITagsRepository tagsRepository = Substitute.For<ITagsRepository>();
            var testData = new List<Tag>()
            {
                new Tag {Name ="c#",Count=2,UsePercentage =50f},
                new Tag {Name ="c++",Count=5, UsePercentage= 2f},
                new Tag {Name ="abs",Count=2,UsePercentage =50f},
                new Tag {Name ="java",Count=5,UsePercentage= 1f},
                new Tag {Name ="dd",Count=5,UsePercentage= 60f}
            }.AsQueryable();
            tagsRepository.GetAllTags().Returns(testData);
            PagedTagDBService pagedTagDBService = new PagedTagDBService(tagsRepository);
            TagParemeters par = new TagParemeters()
            {
                SortBy = "UsePercentage",
                IsDescending = false,
            };
            PagedList<Tag> tags = pagedTagDBService.GetTags(par);
            Assert.Equal(1, tags[0].UsePercentage);
        }
    }
}
