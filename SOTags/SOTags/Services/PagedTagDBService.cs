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
            TagSortingHelper.TagSortingType tagSorting=TagSortingHelper.TagSortingType.ID;
            if (!string.IsNullOrWhiteSpace(tagParemeters.SortBy))
            {
                
                if (tagParemeters.SortBy.Equals("UsePercentage", StringComparison.OrdinalIgnoreCase))
                {
                    tagSorting = TagSortingHelper.TagSortingType.USE_PERCENTAGE;
                }
                else if (tagParemeters.SortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    tagSorting = TagSortingHelper.TagSortingType.NAME;
                }
            }
            List<Tag> tags = _repository.GetTagsPaged(tagParemeters.PageSize, tagParemeters.PageNumber, tagSorting, tagParemeters.IsDescending, out int totalCount);

            pagedTags = PagedList<Tag>.ToPagedList(tags,totalCount,tagParemeters.PageNumber, tagParemeters.PageSize);
            return pagedTags;
        }

    }
}
