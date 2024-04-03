using SOTags.Model;

namespace SOTags.Interfaces
{
    public interface ITagsRepository
    {
        public void AddOrUpdateTagsToDatabase(List<Tag> tags);
        public List<string?> GetTagsName(int pageSize, int pageIndex);
        public int GetNumberOfTagsInDB();
        public List<Tag> GetTagsPaged(int pageSize, int pageNumber, TagSortingHelper.TagSortingType sort, bool isDescending, out int totalCount);
        void CalculateTagsUsage();
    }
}
