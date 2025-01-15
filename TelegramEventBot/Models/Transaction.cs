using System.Text.Json.Serialization;

namespace TelegramEventBot.Models
{
    public class Transaction
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonPropertyName("mcc")]
        public int Mcc { get; set; }

        [JsonPropertyName("originalMcc")]
        public int OriginalMcc { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("operationAmount")]
        public int OperationAmount { get; set; }

        [JsonPropertyName("currencyCode")]
        public int CurrencyCode { get; set; }

        [JsonPropertyName("commissionRate")]
        public int CommissionRate { get; set; }

        [JsonPropertyName("cashbackAmount")]
        public int CashbackAmount { get; set; }

        [JsonPropertyName("balance")]
        public int Balance { get; set; }

        [JsonPropertyName("hold")]
        public bool Hold { get; set; }
    }
}
