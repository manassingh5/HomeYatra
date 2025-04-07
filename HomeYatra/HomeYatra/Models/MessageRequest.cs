using System.Text.Json.Serialization;

namespace HomeYatra.Models
{
    public class MessageRequest
    {
        public string Phone { get; set; }
        public int? TemplateId { get; set; }
        public string? Message { get; set; }
        public string? From { get; set; }
        [JsonIgnore]
        public string? OTP { get; set; }
    }
}
