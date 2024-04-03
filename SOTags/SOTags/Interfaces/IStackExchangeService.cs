namespace SOTags.Interfaces
{
    public interface IStackExchangeService
    {
        public Task<string> GetTagsInfoFromStackExchange(int pageSize, string tagsUrl);
        public Task<string> GetPagedDataFromStackExchange(int pageSize, int pageIndex);
    }
}
