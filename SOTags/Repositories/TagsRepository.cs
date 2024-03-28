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
            AddTagsToDatabase(tags);
        }
        public void AddTagsToDatabase(List<Tag> tags)
        {
            foreach (Tag tag in tags)
            {
                _context.Tags.Add(tag);
            }
            _context.SaveChanges();
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
