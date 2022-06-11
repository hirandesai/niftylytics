using AlphaVantage;
using AlphaVantage.Model;
using CsvHelper;
using NiftyNext50.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NiftyNext50
{
    public class CsvRecordProcessor
    {
        const int NoOfStocksToInvest = 3;
        private readonly AlphaVantageClient alphaVantageClient;
        private readonly CsvWriter priceCardWritter;
        private readonly CsvWriter investmentDetailsWritter;

        public CsvRecordProcessor(AlphaVantageClient alphaVantageClient, CsvWriter priceCardWritter, CsvWriter investmentDetailsWritter)
        {
            this.alphaVantageClient = alphaVantageClient;
            this.priceCardWritter = priceCardWritter;
            this.investmentDetailsWritter = investmentDetailsWritter;
        }

        public async Task ProcessCSVRecords(IEnumerable<CsvReadRecord> csvRecords)
        {
            if (!csvRecords.Any())
            {
                return;
            }
            var processedRecords = new List<CompanyMonthlyReturnRecord>();
            foreach (var record in csvRecords)
            {
                if (string.IsNullOrEmpty(record.Symbol))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Symbol code is empty for Company {record.CompanyName}");
                    Console.ResetColor();
                    continue;
                }
                await ProcessEachReord(processedRecords, record);
            }
            WritePriceCardData(processedRecords);
            ProcessInvestmentResult(processedRecords);
        }

        private async Task ProcessEachReord(List<CompanyMonthlyReturnRecord> processedRecords, CsvReadRecord record)
        {
            var response = await alphaVantageClient.GetMonthlyAdjustedPricesForSymbol(record.AlphaVantageSymbol);
            if (response != null && response.MonthlyAdjustedTimeSeries != null)
            {
                Console.WriteLine($"Data retrived for {record.AlphaVantageSymbol}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Data could not be retrived for {record.AlphaVantageSymbol}");
                Console.ResetColor();
                Console.ReadKey();
            }

            DateTime now = DateTime.Now.AddMonths(-1), tillDate = now.AddYears(-3);
            for (; now > tillDate; now = now.AddMonths(-1))
            {
                int currentYear = now.Year, currentMonth = now.Month;

                var currentMonthRecord = GetTimeSeriesRecordBasedOnYearAndMonth(response, currentYear, currentMonth);
                if (currentMonthRecord == null)
                {
                    continue;
                }
                var processedRecord = new CompanyMonthlyReturnRecord(record)
                {
                    Close = currentMonthRecord.Close,
                    Open = currentMonthRecord.Open,
                    Month = currentMonth,
                    Year = currentYear
                };
                processedRecords.Add(processedRecord);
            }
        }

        private MonthlyAdjustedTimeSeriesRecord GetTimeSeriesRecordBasedOnYearAndMonth(MonthlyAdjustedTimeSeriesResponse timeSeriesData, int year, int month)
        {
            if (timeSeriesData == null || timeSeriesData.MonthlyAdjustedTimeSeries == null)
            {
                return null;
            }
            var key = timeSeriesData.MonthlyAdjustedTimeSeries.Keys.FirstOrDefault(q => q.Year == year && q.Month == month);
            if (key == null || key == DateTime.MinValue)
            {
                return null;
            }
            return timeSeriesData.MonthlyAdjustedTimeSeries[key];
        }

        private void WritePriceCardData(List<CompanyMonthlyReturnRecord> processedRecords)
        {
            var uniqueStocks = processedRecords.Select(s => s.Symbol).Distinct();
            var uniqueDates = processedRecords.Select(s => new { s.Year, s.Month })
                                                .OrderByDescending(o => o.Year)
                                                .ThenByDescending(o => o.Month)
                                                .Distinct();

            priceCardWritter.WriteField("Stock");
            foreach (var header in uniqueDates)
            {
                priceCardWritter.WriteField($"{header.Year}-{header.Month}");
            }
            priceCardWritter.NextRecord();

            foreach (var stockName in uniqueStocks)
            {
                priceCardWritter.WriteField(stockName);
                foreach (var header in uniqueDates)
                {
                    var stock = processedRecords.FirstOrDefault(s => s.Symbol == stockName && s.Year == header.Year && s.Month == header.Month);
                    var stockOpenPrice = stock?.Close;
                    priceCardWritter.WriteField(stockOpenPrice ?? 0);
                }
                priceCardWritter.NextRecord();
            }
        }

        private void ProcessInvestmentResult(List<CompanyMonthlyReturnRecord> processedRecords)
        {
            var investmentResult = new List<CsvWriteRecord>();

            foreach (var month in processedRecords.OrderBy(o => o.Year).ThenBy(o => o.Month)
                                                  .GroupBy(g => new { g.Month, g.Year }))
            {
                var stocksToInvest = month.OrderByDescending(o => o.Return)
                                          .Take(NoOfStocksToInvest)
                                          .ToList();
                var stockWithHigestValue = stocksToInvest.FirstOrDefault(s => s.Open == stocksToInvest.Select(s => s.Open).Max());
                investmentResult.Add(new CsvWriteRecord(stockWithHigestValue)
                {
                    Qty = 1
                });
                foreach (var stockToInvest in stocksToInvest.Except(new CompanyMonthlyReturnRecord[] { stockWithHigestValue }))
                {
                    investmentResult.Add(new CsvWriteRecord(stockToInvest)
                    {
                        Qty = (int)Math.Round(stockWithHigestValue.Open/stockToInvest.Open)
                    });
                }

            }
            investmentDetailsWritter.WriteRecords(investmentResult);
        }
    }
}
