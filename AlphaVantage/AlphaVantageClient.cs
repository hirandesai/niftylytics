using AlphaVantage.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AlphaVantage
{
    public class AlphaVantageClient
    {
        const int MinIntervalBetweenAPI = 15 * 1000;
        const string RootAlphaVantageURL = "https://www.alphavantage.co/";
        private readonly HttpClient httpClient;

        private readonly string apiKey;

        private readonly Stopwatch stopwatch;

        public AlphaVantageClient(string apiKey)
        {
            this.apiKey = apiKey;
            httpClient = new HttpClient()
            {
                BaseAddress = new Uri(RootAlphaVantageURL)
            };
            stopwatch = new Stopwatch();
        }

        public async Task<MonthlyAdjustedTimeSeriesResponse> GetMonthlyAdjustedPricesForSymbol(string symbol)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, $"{symbol}.json");
            if (File.Exists(filePath) && File.GetLastWriteTime(filePath) > DateTime.Now.Date)
            {
                return JsonConvert.DeserializeObject<MonthlyAdjustedTimeSeriesResponse>(File.ReadAllText(filePath));
            }


            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 0 && stopwatch.ElapsedMilliseconds < MinIntervalBetweenAPI)
            {
                var delayTime = (int)(MinIntervalBetweenAPI - stopwatch.ElapsedMilliseconds);
                await Task.Delay(delayTime);
            }
            string RequestUrlMonthlyAdjusted = $"query?function=TIME_SERIES_MONTHLY_ADJUSTED&symbol={symbol}&apikey={apiKey}";
            var response = await httpClient.GetAsync(RequestUrlMonthlyAdjusted);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            stopwatch.Reset();
            stopwatch.Start();
            File.WriteAllText(filePath, jsonResponse);
            return JsonConvert.DeserializeObject<MonthlyAdjustedTimeSeriesResponse>(jsonResponse);
        }
    }
}
