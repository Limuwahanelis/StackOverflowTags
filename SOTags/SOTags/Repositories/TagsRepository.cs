using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SOTags.Data;
using SOTags.Interfaces;
using SOTags.Model;
using SOTags.Services;

namespace SOTags.Repositories
{
    public class TagsRepository:ITagsRepository
    {
        private readonly SOTagsDBContext _context;
        public TagsRepository(SOTagsDBContext context)
        {
            _context = context;
        }
        public void AddOrUpdateTagsToDatabase(List<Tag> tags)
        {
            _context.Database.EnsureCreated();
            foreach (Tag tag in tags)
            {
                Tag? oldTag = _context.Tags.Where(t => t.Name == tag.Name).FirstOrDefault();
                if (oldTag != null)
                {
                    if (oldTag.Count != tag.Count) UpdateTag(oldTag, tag);
                }
                else
                {
                    _context.Tags.Add(tag);
                    Log.Information("Added new tag {tagName} with count {tagCount}", tag.Name, tag.Count);
                }
            }
            _context.SaveChanges();
        }
        private void UpdateTag(Tag oldTag,Tag newTag)
        {
            oldTag.Name = newTag.Name;
            Log.Information("Updated {oldtag} count from {oldCount} to {newCount}", oldTag.Name, oldTag.Count, newTag.Count);
            oldTag.Count = newTag.Count;
        }
        public int GetNumberOfTagsInDB()
        {
            return _context.Tags.Count();
        }
        public List<string> GetTagsName(int pageSize,int pageIndex)
        {
            List<string> toReturn;
            toReturn = _context.Tags.OrderByDescending(t=>t.Count).Skip(pageSize* pageIndex).Take(pageSize).Select(t=>t.Name).ToList();
            return toReturn;
        }
        public void CalculateTagsUsage()
        {
            long totalNumberOfTagsUse = 0;
            foreach (var tag in _context.Tags)
            {
                totalNumberOfTagsUse += tag.Count;
            }
            foreach (var tag in _context.Tags)
            {
                tag.UsePercentage= float.Round(100f * tag.Count / totalNumberOfTagsUse, 2);
            }
            _context.SaveChanges();
            Log.Information("{totaltagsCount} tags count in repo", totalNumberOfTagsUse);
        }
        public List<Tag> GetTagsPaged(int pageSize, int pageNumber,TagSortingHelper.TagSortingType sort,bool isDescending,out int totalCount)
        {
            List<Tag> toReturn;
            totalCount = GetNumberOfTagsInDB();
            int TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            orderByFunc = tag => tag.Id;
            switch (sort)
            {
                case TagSortingHelper.TagSortingType.USE_PERCENTAGE: orderByFunc = tag => tag.UsePercentage;break;
                case TagSortingHelper.TagSortingType.NAME: orderByFunc = tag => tag.Name; break;
                case TagSortingHelper.TagSortingType.ID: orderByFunc = tag => tag.Id; break;
            }
               
            toReturn = (isDescending?_context.Tags.AsNoTracking().OrderByDescending(orderByFunc):_context.Tags.OrderBy(orderByFunc)).Skip(pageSize*(pageNumber-1)).Take(pageSize).ToList();
            return toReturn;
        }
        Func<Tag, Object>? orderByFunc;
    }
}
