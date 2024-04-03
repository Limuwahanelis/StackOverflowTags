using Microsoft.IdentityModel.Tokens;
using Serilog;
using SOTags.Exceptions;
using SOTags.Interfaces;
using SOTags.Model;
using System.Reflection;
using System.Text.Json;
using System.Web;

namespace SOTags.Services
{
    public class StackExchangeTagDBService
    {
        private readonly ITagsRepository _tagsRepository;
        private readonly IStackExchangeService _stackExchangeService;
        private readonly IConfiguration _configuration;
        int _pageSize;
        int _numberOfTagsToImport;

        public StackExchangeTagDBService(ITagsRepository tagsRepository, IStackExchangeService stackExchangeService, IConfiguration configuration)
        {
            _tagsRepository = tagsRepository;
            _stackExchangeService = stackExchangeService;
            _configuration = configuration;
            _pageSize = _configuration.GetValue<int>("StackExchangeServer:PageSize");
            _numberOfTagsToImport = _configuration.GetValue<int>("StackExchangeServer:NumberOfRequiredTags");
        }
        public async Task ImportTagsFromStackOverflow()
        {
            int pageIndex = 1;
            string data = "";
            for (int numberOfEntries = 0; numberOfEntries < _numberOfTagsToImport;)
            {
                try
                {
                    data = await _stackExchangeService.GetPagedDataFromStackExchange(_pageSize, pageIndex);
                }
                catch (StackExchangeServerCouldNotBeReachedException ex)
                {
                    ex.OperationMessage = $"Imported or updated {numberOfEntries} tags from required {_numberOfTagsToImport} entries";
                    throw;
                }
                List<Tag> tags = GetTagsFromStackExchangeResponse(data);
                _tagsRepository.AddOrUpdateTagsToDatabase(tags);
                numberOfEntries += tags.Count;
                if (!JsonDocument.Parse(data).RootElement.GetProperty("has_more").GetBoolean()) break;
                pageIndex++;
            }
            _tagsRepository.CalculateTagsUsage();
        }
        private List<Tag> GetTagsFromStackExchangeResponse(string data)
        {
            List<Tag> tags = new List<Tag>();
            JsonDocument jsonDocument = JsonDocument.Parse(data);
            JsonElement tagsList = jsonDocument.RootElement.GetProperty("items");
            foreach (JsonElement element in tagsList.EnumerateArray())
            {
                Tag? tag = element.Deserialize<Tag>();
                if(tag!=null) tags.Add(tag);
            }
            return tags;
        }
    }
}
