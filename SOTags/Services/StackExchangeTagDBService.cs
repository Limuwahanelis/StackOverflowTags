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

        public StackExchangeTagDBService(ITagsRepository tagsRepository, IStackExchangeService stackExchangeService)
        {
            _tagsRepository = tagsRepository;
            _stackExchangeService = stackExchangeService;
        }
        public async Task ImportTagsFromStackOverflow(int numberOfTagsToImport)
        {
            int pageSize = 100;
            int pageIndex = 1;
            string data = "";
            for (int numberOfEntries = 0; numberOfEntries < numberOfTagsToImport;)
            {
                try
                {
                    data = await _stackExchangeService.GetPagedDataFromStackExchange(pageSize, pageIndex);
                }
                catch (StackExchangeServerCouldNotBeReachedException ex)
                {
                    ex.OperationMessage = $"Imported or updated {numberOfEntries} tags from required {numberOfTagsToImport} entries";
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
            int minNumberOftagsTOUpdate = 1000;
            int pageSize = 100;
            int pageIndex = 1;
            List<Tag> tags;
            string data = "";
            int numberOfTagsUpdated = 0;
            List<string?> allNames= new List<string?>();
            while ( numberOfTagsUpdated < _tagsRepository.GetNumberOfTagsInDB())
            {
                List<string?> tagsNames = _tagsRepository.GetTagsName(pageSize, pageIndex-1);
                allNames.AddRange(tagsNames);
                string tagsToUpdate = string.Join(";", tagsNames);
                // If tags contains some special characters it needs to be properly encoded for request to work
                string tagsUrl = HttpUtility.UrlEncode(tagsToUpdate);
                try
                {
                    data = await _stackExchangeService.GetTagsInfoFromStackExchange(pageSize, tagsUrl);
                }
                catch (StackExchangeServerCouldNotBeReachedException ex)
                {
                    ex.OperationMessage = $"Updated {numberOfTagsUpdated} tags from {_tagsRepository.GetNumberOfTagsInDB()}";
                    throw;
                }
                tags = GetTagsFromStackExchangeResponse(data);
                _tagsRepository.AddOrUpdateTagsToDatabase(tags);
                numberOfTagsUpdated += tags.Count;
                pageIndex++;
            }
            pageIndex = 1;
            // if for some reason database lacks required 1000 tags, get the rest from stack exchange
            if (numberOfTagsUpdated< minNumberOftagsTOUpdate)
            {
                int numberOfEntries = numberOfTagsUpdated;
                while (numberOfEntries < minNumberOftagsTOUpdate)
                {
                    try
                    {
                        data = await _stackExchangeService.GetPagedDataFromStackExchange(pageSize, pageIndex);
                    }
                    catch (StackExchangeServerCouldNotBeReachedException ex)
                    {
                        ex.OperationMessage = $"Imported or updated {numberOfEntries} tags from required {minNumberOftagsTOUpdate} entries";
                        throw;
                    }
                    tags = GetTagsFromStackExchangeResponse(data);
                    if (numberOfEntries + tags.Count() > minNumberOftagsTOUpdate)
                    {
                        tags.RemoveRange(minNumberOftagsTOUpdate - numberOfEntries, tags.Count() - (minNumberOftagsTOUpdate - numberOfEntries));
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
    }
}
