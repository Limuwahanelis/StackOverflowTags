using Microsoft.EntityFrameworkCore;
using SOTags.CustomDataFormats;
using SOTags.Data;
using SOTags.Model;

namespace SOTags.Services
{
    public class TagDBService
    {
        private readonly SOTagsDBContext _context;
        public TagDBService(SOTagsDBContext context)
        {
            _context = context;
        }

        public PagedList<Tag> GetTags(TagParemeters tagParemeters)
        {
            using (_context)
            {
                PagedList<Tag> pagedTags;
                var tags = from t in _context.Tags
                           select t;
                if(!string.IsNullOrWhiteSpace(tagParemeters.SortBy))
                {
                    if (tagParemeters.SortBy.Equals("UsePercentage",StringComparison.OrdinalIgnoreCase))
                    {
                        tags = tagParemeters.IsDescending ? tags.OrderByDescending(t => t.UsePercentage) : tags.OrderBy(t => t.UsePercentage);
                    }
                    else if(tagParemeters.SortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    {
                        tags = tagParemeters.IsDescending ? tags.OrderByDescending(t => t.Name) : tags.OrderBy(t => t.Name);
                    }
                }
                pagedTags = PagedList<Tag>.ToPagedList(tags.AsNoTracking(),tagParemeters.PageNumber,tagParemeters.PageSize);
                return pagedTags;
            }
            
        }
    }
}
