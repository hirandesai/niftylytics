using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AlphaVantage.Model
{

    public class MonthlyAdjustedTimeSeriesResponse
    {
        [JsonProperty("Meta Data")]
        public Dictionary<string,string> MetaData { get; set; }

        [JsonProperty("Monthly Adjusted Time Series")]
        public Dictionary<DateTime, MonthlyAdjustedTimeSeriesRecord> MonthlyAdjustedTimeSeries { get; set; }
    }

    public class MonthlyAdjustedTimeSeriesRecord
    {
        [JsonProperty("1. open")]
        public decimal Open { get; set; }

        [JsonProperty("2. high")]
        public decimal High { get; set; }

        [JsonProperty("3. low")]
        public decimal Low { get; set; }

        [JsonProperty("4. close")]
        public decimal Close { get; set; }

        [JsonProperty("5. adjusted close")]
        public decimal AdjustedClose { get; set; }

        [JsonProperty("6. volume")]
        public decimal Volumn { get; set; }

        [JsonProperty("7. dividend amount")]
        public decimal DividedAmount { get; set; }
    }

}
