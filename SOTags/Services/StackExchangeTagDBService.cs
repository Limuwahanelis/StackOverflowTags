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

        public async Task UpdateTagsInDB()
        {
            int pageIndex = 1;
            List<Tag> tags;
            string data = "";
            int numberOfTagsUpdated = 0;
            List<string?> allNames= new List<string?>();
            while ( numberOfTagsUpdated < _tagsRepository.GetNumberOfTagsInDB())
            {
                List<string?> tagsNames = new List<string?> { "graph", "chart", "c++", "charts" }; //_tagsRepository.GetTagsName(pageSize, pageIndex-1);
                allNames.AddRange(tagsNames);
                string tagsToUpdate = string.Join(";", tagsNames);
                // If tags contains some special characters it needs to be properly encoded for request to work
                string tagsUrl = HttpUtility.UrlEncode(tagsToUpdate);
                try
                {
                    data = await _stackExchangeService.GetTagsInfoFromStackExchange(_pageSize, tagsUrl);
                }
                catch (StackExchangeServerCouldNotBeReachedException ex)
                {
                    ex.OperationMessage = $"Updated {numberOfTagsUpdated} tags from {_tagsRepository.GetNumberOfTagsInDB()}";
                    throw;
                }
                tags = GetTagsFromStackExchangeResponse(data,tagsNames);
                _tagsRepository.AddOrUpdateTagsToDatabase(tags);
                numberOfTagsUpdated += tags.Count;
                pageIndex++;
            }
            pageIndex = 1;
            // if for some reason database lacks required tags, get the rest from stack exchange
            if (numberOfTagsUpdated < _numberOfTagsToImport)
            {
                int numberOfEntries = numberOfTagsUpdated;
                while (numberOfEntries < _numberOfTagsToImport)
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
                    tags = GetTagsFromStackExchangeResponse(data);
                    if (numberOfEntries + tags.Count() > _numberOfTagsToImport)
                    {
                        tags.RemoveRange(_numberOfTagsToImport - numberOfEntries, tags.Count() - (_numberOfTagsToImport - numberOfEntries));
                    }
                    //Console.WriteLine($"total tags usage = {totalNumberOfTagsUse}");
                    _tagsRepository.AddOrUpdateTagsToDatabase(tags);
                    int differnce = _tagsRepository.GetNumberOfTagsInDB() - numberOfEntries;
                    numberOfEntries += differnce;
                    Log.Information("Added {tagsNumber} tags during tag updating", differnce);
                    if (!JsonDocument.Parse(data).RootElement.GetProperty("has_more").GetBoolean()) break;
                    pageIndex++;
                }
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
                Tag tag = element.Deserialize<Tag>();
                tags.Add(tag);
            }
            return tags;
        }
        private List<Tag> GetTagsFromStackExchangeResponse(string data,List<string> tagNames)
        {
            List<Tag> tags = new List<Tag>();
            JsonDocument jsonDocument = JsonDocument.Parse(data);
            JsonElement tagsList = jsonDocument.RootElement.GetProperty("items");
            foreach (JsonElement element in tagsList.EnumerateArray())
            {
                Tag tag = element.Deserialize<Tag>();
                JsonElement synonyms;
                bool hasSynonyms = element.TryGetProperty("synonyms",out synonyms);
                if (hasSynonyms)
                {
                    foreach(JsonElement element1 in synonyms.EnumerateArray())
                    {
                        
                        if(tagNames.Contains(element1.GetString()))
                        {
                            if (tag.Name.Equals(element1.GetString())) continue;
                            Tag synonymTag = new Tag()
                            { 
                                Count = tag.Count,
                                Name = element1.GetString()
                            };
                            tags.Add(synonymTag);
                        }
                    }
                }
                tags.Add(tag);
            }
            return tags;
        }
    }
}
