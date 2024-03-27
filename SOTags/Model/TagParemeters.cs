namespace SOTags.Model
{
    public class TagParemeters
    {
        private const int _maxPageSize = 50;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        public string? SortBy {  get; set; } = null;
        public bool IsDescending { get; set; } = false;
        public int PageSize
        { 
            get { return _pageSize;} 
            set { _pageSize = (value>_maxPageSize)?_maxPageSize:value; }
        }
    }
}
