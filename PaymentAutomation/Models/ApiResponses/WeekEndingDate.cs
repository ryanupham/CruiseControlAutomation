using PaymentAutomation.JsonConverters;
using System.Text.Json.Serialization;

namespace PaymentAutomation.Models.ApiResponses
{
    internal record WeekEndingDate
    {
        [JsonPropertyName("weekEndingDate")]
        public DateOnly Date { get; init; }
        [JsonPropertyName("canBePosted")]
        public bool CanBePosted { get; init; }
        [JsonPropertyName("displayReport")]
        public bool DisplayReport { get; init; }
        [JsonPropertyName("valid")]
        public bool Valid { get; init; }
    }
}
