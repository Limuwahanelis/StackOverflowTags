using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Frameworks;
using SOTags.Data;
using SOTags.Model;
using SOTags.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SOTags.UnitTest
{
    
    public class TagsRepositoryTests
    {
        string connectionString = "Server=(localdb)\\mssqllocaldb;Database=TagsRepoTestDB;Trusted_Connection=True;";

        [Fact]
        public void GetTagsPaged_SortedByNameAscendingPagseSize25Number2_RetrunsDocker()
        {
            SOTagsDBContext context = new SOTagsDBContext(new DbContextOptionsBuilder<SOTagsDBContext>()
            .UseSqlServer(connectionString).Options);
            List<Tag> tags=new List<Tag>();
            TagParemeters tagParemeters = new TagParemeters()
            {
                PageSize = 25,
                PageNumber = 2
            };

            using (context) 
            {

                context.Database.EnsureCreated();
                if (context.Tags.Count() == 0) AddTagsToTestDB(context);

                TagsRepository repo = new TagsRepository(context);
                repo.CalculateTagsUsage();
                tags= repo.GetTagsPaged(tagParemeters.PageSize,tagParemeters.PageNumber,TagSortingHelper.TagSortingType.NAME,false,out int count);

            }
                Assert.Equal("docker",tags.First().Name);
        }

        [Fact]
        public void GetTagsPaged_SortedByPercantageUseDescendingPagseSize30Number3_RetrunsMultithreading()
        {
            SOTagsDBContext context = new SOTagsDBContext(new DbContextOptionsBuilder<SOTagsDBContext>()
            .UseSqlServer(connectionString).Options);
            List<Tag> tags = new List<Tag>();
            TagParemeters tagParemeters = new TagParemeters()
            {
                PageSize = 30,
                PageNumber = 3
            };

            using (context)
            {

                context.Database.EnsureCreated();
                if (context.Tags.Count() == 0) AddTagsToTestDB(context);

                TagsRepository repo = new TagsRepository(context);
                repo.CalculateTagsUsage();
                tags = repo.GetTagsPaged(tagParemeters.PageSize, tagParemeters.PageNumber, TagSortingHelper.TagSortingType.USE_PERCENTAGE, true, out int count);

            }
            Assert.Equal("multithreading", tags.First().Name);
        }
        [Fact]
        public void CalculateTagsUsage_Retruns7dot67()
        {
            SOTagsDBContext context = new SOTagsDBContext(new DbContextOptionsBuilder<SOTagsDBContext>()
            .UseSqlServer(connectionString).Options);
            using (context)
            {

                context.Database.EnsureCreated();
                if (context.Tags.Count() == 0) AddTagsToTestDB(context);

                TagsRepository repo = new TagsRepository(context);
                repo.CalculateTagsUsage();

                Assert.Equal(float.Round(7.67f,2), context.Tags.AsNoTracking().OrderByDescending(t=>t.UsePercentage).First().UsePercentage);
            }
        }
        private void AddTagsToTestDB(SOTagsDBContext context)
        {
            var absPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string data = File.ReadAllText(absPath + "/Jsons/data.json");
            List<Tag> newTags = GetTags(data);
            context.Tags.AddRange(newTags);
            context.SaveChanges();
        }
        private List<Tag> GetTags(string data)
        {
            List<Tag> tags = new List<Tag>();
            JsonDocument jsonDocument = JsonDocument.Parse(data);
            JsonElement tagsList = jsonDocument.RootElement.GetProperty("items");
            foreach (JsonElement element in tagsList.EnumerateArray())
            {
                Tag tag = element.Deserialize<Tag>();
                tags.Add(tag);
            }
            return tags;
        }
    }
}
