using SOTags.Model;

namespace SOTags.Interfaces
{
    public interface ITagsRepository
    {
        public void AddTagsToDatabase(List<Tag> tags);
        public void CalculateTagsUsage(long totalNumberOfTagsUse);
        public void InitalizeDatabase(List<Tag> tags);
        public IQueryable<Tag> GetAllTags();
    }
}
