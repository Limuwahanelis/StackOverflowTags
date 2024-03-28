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
        public void InitalizeDatabase(List<Tag> tags)
        {
            _context.Database.EnsureCreated();
            if (!_context.Tags.IsNullOrEmpty()) return;
            AddOrUpdateTagsToDatabase(tags);
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
            oldTag.Name = newTag.Name;
            oldTag.Count = newTag.Count;
        }
        public int GetNumberOfTagsInDB()
        {
            return _context.Tags.Count();
        }
        public List<string?> GetTagsName(int fromId, int toId)
        {
            List<string?> toReturn;
            toReturn = _context.Tags.Where(tag => tag.Id >= fromId && tag.Id <= toId).Select(tag => tag.Name).ToList();
            return toReturn;
        }
        public void CalculateTagsUsage(long totalNumberOfTagsUse)
        {
            foreach (var tag in _context.Tags)
            {
                tag.UsePercentage = float.Round(100f * tag.Count / totalNumberOfTagsUse, 2);
                Console.WriteLine(tag.UsePercentage);
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
