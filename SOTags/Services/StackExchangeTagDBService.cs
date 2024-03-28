using SOTags.Exceptions;
using SOTags.Interfaces;
using SOTags.Model;
using System.Text.Json;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SOTags.Services
{
    public class StackExchangeTagDBService
    {
        private readonly ITagsRepository _tagsRepository;
        private readonly StackExchangeService _stackExchangeService;

        public StackExchangeTagDBService(ITagsRepository tagsRepository, StackExchangeService stackExchangeService)
        {
            _tagsRepository = tagsRepository;
            _stackExchangeService = stackExchangeService;
        }
        public async Task ImportTagsFromStackOverflow()
        {
            
            //if (true) return;
            int pageSize = 100;
            int pageIndex = 1;
            long totalNumberOfTagsUse = 0;
            string data = "";
            for (int numberOfEntries = 0; numberOfEntries < 1000;)
            {
                try
                {
                    data = await _stackExchangeService.GetPagedDataFromStackExchange(pageSize, pageIndex);
                }
                catch (StackExchangeServerCouldNotBeReachedException ex)
                {
                    ex.OperationMessage = $"Imported or updated {numberOfEntries} tags from required {1000} entries";
                    throw;
                }
                List<Tag> tags = GetTagsFromStackExchangeResponse(data, ref totalNumberOfTagsUse);
                //Console.WriteLine($"total tags usage = {totalNumberOfTagsUse}");
                _tagsRepository.AddOrUpdateTagsToDatabase(tags);
                numberOfEntries += tags.Count;
                if (!JsonDocument.Parse(data).RootElement.GetProperty("has_more").GetBoolean()) break;
                pageIndex++;
            }
            Console.WriteLine($"total tags usage = {totalNumberOfTagsUse}");
            _tagsRepository.CalculateTagsUsage(totalNumberOfTagsUse);
        }

        public async Task UpdateTagsInDB()
        {
            int pageSize = 100;
            int pageIndex = 1;
            long totalNumberOfTagsUse = 0;
            List<Tag> tags;
            string data = "";
            for (int numberOfTagsUpdated = 0; numberOfTagsUpdated < _tagsRepository.GetNumberOfTagsInDB();)
            {
                List<string?> tagsNames = _tagsRepository.GetTagsName(pageSize, (pageIndex-1)*pageSize);//(1 + (pageIndex - 1) * pageSize, pageIndex * pageSize);
                string tagsToUpdate = string.Join(";", tagsNames);

                // If tags contains some special characters it needs to be properly encoded for request to work
                string tagsUrl = HttpUtility.UrlEncode(tagsToUpdate);
                try
                {
                   data = await _stackExchangeService.GetTagsInfoFromStackExchange(pageSize, tagsUrl);
                } catch (StackExchangeServerCouldNotBeReachedException ex) 
                {
                    ex.OperationMessage = $"Updated {numberOfTagsUpdated} from {_tagsRepository.GetNumberOfTagsInDB()}";
                    throw;
                }
                tags = GetTagsFromStackExchangeResponse(data, ref totalNumberOfTagsUse);
                //Console.WriteLine($"total tags usage = {totalNumberOfTagsUse}");
                _tagsRepository.AddOrUpdateTagsToDatabase(tags);
                numberOfTagsUpdated += tagsNames.Count;
                pageIndex++;
            }
            Console.WriteLine($"total tags usage = {totalNumberOfTagsUse}");
            _tagsRepository.CalculateTagsUsage(totalNumberOfTagsUse);
        }
        private List<Tag> GetTagsFromStackExchangeResponse(string data, ref long totalNumberOfTagsUse)
        {
            List<Tag> tags = new List<Tag>();
            JsonDocument jsonDocument = JsonDocument.Parse(data);
            JsonElement tagsList = jsonDocument.RootElement.GetProperty("items");
            foreach (JsonElement element in tagsList.EnumerateArray())
            {
                Tag tag = element.Deserialize<Tag>();
                tags.Add(tag);
                totalNumberOfTagsUse += tag.Count;
                //Console.WriteLine($"Count {tag.Count} from tag {tag.Name}, total tags usage = {totalNumberOfTagsUse}");
            }
            return tags;
        }
    }
}
