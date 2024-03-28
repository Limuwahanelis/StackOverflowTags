using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
                if (oldTag != null) UpdateTag(oldTag, tag);
                else _context.Tags.Add(tag);
            }
            _context.SaveChanges();
        }
        public void UpdateTag(Tag oldTag,Tag newTag)
        {
            if(oldTag.Count != newTag.Count)
            {
                Console.WriteLine($"new tag differ by {newTag.Count - oldTag.Count}");
            }
            oldTag.Name = newTag.Name;
            oldTag.Count = newTag.Count;
        }
        public int GetNumberOfTagsInDB()
        {
            return _context.Tags.Count();
        }
        public List<string?> GetTagsName(int pageSize,int lastId)
        {
            List<string?> toReturn;
            toReturn = _context.Tags.OrderBy(t=>t.Id).Where(t=>t.Id>lastId).Take(pageSize).Select(t=>t.Name).ToList();
            return toReturn;
        }
        public void CalculateTagsUsage(long totalNumberOfTagsUse)
        {
            foreach (var tag in _context.Tags)
            {
                tag.UsePercentage = float.Round(100f * tag.Count / totalNumberOfTagsUse, 2);
            }
            _context.SaveChanges();
        }
        public IQueryable<Tag> GetAllTags()
        {
            var tags = from t in _context.Tags
                       select t;
            return tags;
        }
    }
}
