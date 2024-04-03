namespace SOTags.Interfaces
{
    public interface IStackExchangeService
    {
        public Task<string> GetPagedDataFromStackExchange(int pageSize, int pageIndex);
    }
}
