using SOTags.Model;

namespace SOTags.Interfaces
{
    public interface ITagsRepository
    {
        public void AddOrUpdateTagsToDatabase(List<Tag> tags);
        public void CalculateTagsUsage(long totalNumberOfTagsUse);
        public List<string?> GetTagsName(int pageSize, int lastId);
        public int GetNumberOfTagsInDB();
        public IQueryable<Tag> GetAllTags();
    }
}
