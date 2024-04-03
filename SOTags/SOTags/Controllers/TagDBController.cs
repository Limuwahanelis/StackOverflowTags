using Microsoft.AspNetCore.Mvc;
using SOTags.CustomDataFormats;
using SOTags.Data;
using SOTags.Model;
using SOTags.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOTags.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TagDBController:ControllerBase
    {
        private readonly PagedTagDBService _tagDBService;
        private readonly StackExchangeTagDBService _stackExchangeTagDBService;
        public TagDBController(PagedTagDBService tagDBService, StackExchangeTagDBService stackExchangeService)
        {
            _tagDBService = tagDBService;
            _stackExchangeTagDBService = stackExchangeService;
        }

        [HttpGet]
        public ActionResult<PagedList<Tag>> GetTags([FromQuery] TagParemeters tagParemeters)
        {

            PagedList<Tag> tags = _tagDBService.GetTags(tagParemeters);

            var metadata = new
            {
                tags.TotalCount,
                tags.PageSize,
                tags.CurrentPage,
                tags.TotalPages,
                tags.HasNext,
                tags.HasPrevious,
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));

            return Ok(tags);
        }
        [HttpGet("Update")]
        public async Task<ActionResult> UpdateTagsDB()
        {
           await _stackExchangeTagDBService.UpdateTagsInDB();

           return Ok("Updated");
        }
    }
}
