using System.Text.Json.Serialization;

namespace CurrencyConverter.WebAPI.Models
{
    public class CurrencyRates
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("base")]
        public string Base { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<DateTime, Dictionary<string, decimal>> Rates { get; set; }
    }
}
