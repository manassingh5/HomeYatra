using System.Text.Json.Serialization;

namespace HomeYatra.Models
{
   public class Language
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
    }

    public class Template
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("language")]
        public Language Language { get; set; }

        [JsonPropertyName("components")]
        public List<Component> Components { get; set; }

    }
    public class SendMessageTemplate
    {

        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("recipient_type")]
        public string recipient_type { get; set; }

        [JsonPropertyName("template")]
        public Template Template { get; set; }

        //[JsonIgnore]
        //public Template1? Template1 { get; set; }
    }

    public class Parameter
    {
        public string type { get; set; }
        public Image image { get; set; }
        public string text { get; set; }
    }

    public class Image
    {
        public string id { get; set; }
    }

    public class Component
    {
        public string type { get; set; }
        public string sub_type { get; set; }
        public string index { get; set; }
        public List<Parameter> parameters { get; set; }
    }
}

