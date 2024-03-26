using System.Text.Json.Serialization;

namespace SOTags.Model
{
    public class Tag
    {
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
