using Microsoft.EntityFrameworkCore;
using SOTags.CustomDataFormats;
using SOTags.Data;
using SOTags.Interfaces;
using SOTags.Model;
using SOTags.Repositories;

namespace SOTags.Services
{
    public class PagedTagDBService
    {
        private readonly ITagsRepository _repository;
        public PagedTagDBService(ITagsRepository repository)
        {
            _repository = repository;
        }

        public PagedList<Tag> GetTags(TagParemeters tagParemeters)
        {
            PagedList<Tag> pagedTags;
            var tags = _repository.GetAllTags();
            if (!string.IsNullOrWhiteSpace(tagParemeters.SortBy))
            {
                if (tagParemeters.SortBy.Equals("UsePercentage", StringComparison.OrdinalIgnoreCase))
                {
                    tags = tagParemeters.IsDescending ? tags.OrderByDescending(t => t.UsePercentage) : tags.OrderBy(t => t.UsePercentage);
                }
                else if (tagParemeters.SortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    tags = tagParemeters.IsDescending ? tags.OrderByDescending(t => t.Name) : tags.OrderBy(t => t.Name);
                }
            }
            pagedTags = PagedList<Tag>.ToPagedList(tags.AsNoTracking(), tagParemeters.PageNumber, tagParemeters.PageSize);
            return pagedTags;
        }

    }
}
