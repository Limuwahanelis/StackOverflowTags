using SOTags.Model;

namespace SOTags.Interfaces
{
    public interface ITagsRepository
    {
        public void AddOrUpdateTagsToDatabase(List<Tag> tags);
        public void CalculateTagsUsage(long totalNumberOfTagsUse);
        public void InitalizeDatabase(List<Tag> tags);
        public List<string?> GetTagsName(int fromId, int toId);
        public int GetNumberOfTagsInDB();
        public IQueryable<Tag> GetAllTags();
    }
}
