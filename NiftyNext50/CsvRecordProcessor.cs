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
        private readonly AlphaVantageClient _alphaVantageClient;
        private readonly CsvWriter _priceCardWriter;
        private readonly CsvWriter _investmentDetailsWriter;

        public CsvRecordProcessor(AlphaVantageClient alphaVantageClient, CsvWriter priceCardWriter, CsvWriter investmentDetailsWriter)
        {
            _alphaVantageClient = alphaVantageClient;
            _priceCardWriter = priceCardWriter;
            _investmentDetailsWriter = investmentDetailsWriter;
        }

        public async Task ProcessCsvRecords(IList<CsvReadRecord> csvRecords, bool considerSell = false, bool shouldSellEveryThingToday = false)
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
                await ProcessEachRecord(processedRecords, record);
            }
            WritePriceCardData(processedRecords);
            ProcessInvestmentResult(processedRecords, considerSell, shouldSellEveryThingToday);
        }

        private async Task ProcessEachRecord(List<CompanyMonthlyReturnRecord> processedRecords, CsvReadRecord record)
        {
            var response = await _alphaVantageClient.GetMonthlyAdjustedPricesForSymbol(record.AlphaVantageSymbol);
            if (response != null && response.MonthlyAdjustedTimeSeries != null)
            {
                Console.WriteLine($"Data retrieved for {record.AlphaVantageSymbol}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Data could not be retrieved for {record.AlphaVantageSymbol}");
                Console.ResetColor();
                return;
            }

            DateTime now = DateTime.Now.AddMonths(-1), tillDate = now.AddYears(-12);
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

            _priceCardWriter.WriteField("Stock");
            foreach (var header in uniqueDates)
            {
                _priceCardWriter.WriteField($"{header.Year}-{header.Month}");
            }
            _priceCardWriter.NextRecord();

            foreach (var stockName in uniqueStocks)
            {
                _priceCardWriter.WriteField(stockName);
                foreach (var header in uniqueDates)
                {
                    var stock = processedRecords.FirstOrDefault(s => s.Symbol == stockName && s.Year == header.Year && s.Month == header.Month);
                    var stockOpenPrice = stock?.Close; //todo: why is this closing price? Should this not be open price?
                    _priceCardWriter.WriteField(stockOpenPrice ?? 0);
                }
                _priceCardWriter.NextRecord();
            }
        }

        /// <summary>
        /// For each month and year, identify stocks with highest return, take the first X
        /// Select the one which has highest investment value - invest 1 qty in it
        /// Invest in other two in proportionate to the highest with the price!
        /// </summary>
        /// <param name="processedRecords">Alphavintage stock market data</param>
        /// <param name="considerSell">If false, the analaysis will be done only for purchase. If true, the analysis will be done for sell
        /// also such that the stock is in bottom 3 (as per their return in the previous month then it will be sold)</param>
        private void ProcessInvestmentResult(List<CompanyMonthlyReturnRecord> processedRecords, bool considerSell = false, bool shouldSellEveryThingToday = false)
        {
            var investmentResult = new List<CsvWriteRecord>();
            var allPreviousInvestments = new List<CsvWriteRecord>();
            foreach (var month in processedRecords.OrderBy(o => o.Year)
                         .ThenBy(o => o.Month)
                         .GroupBy(g => new { g.Month, g.Year }))
            {
                ProcessStocksForInvestments(month, investmentResult);
                if (considerSell)
                {
                    ProcessStocksForDisinvestment(month, investmentResult);
                }
            }

            if (shouldSellEveryThingToday)
            {
                ProcessStocksForFinalSaleReturn(processedRecords, investmentResult);
            }

            _investmentDetailsWriter.WriteRecords(investmentResult);
        }

        private static void ProcessStocksForInvestments(IGrouping<dynamic, CompanyMonthlyReturnRecord> month, List<CsvWriteRecord> investmentResult)
        {
            var stocksToInvest = month.Where(q => q.Return > 0)
                                                                    .OrderByDescending(o => o.Return)
                                                                    .Take(NoOfStocksToInvest)
                                                                    .ToList();
            var stockWithHighestInvestmentValue = stocksToInvest
                                                                            .FirstOrDefault(s => s.Open == stocksToInvest.Select(s => s.Open).Max());
            if (stockWithHighestInvestmentValue == null)
            {
                return;
            }

            investmentResult.Add(new CsvWriteRecord(stockWithHighestInvestmentValue)
            {
                PurchaseQty = 1
            });

            foreach (var stockToInvest in stocksToInvest.Except(new CompanyMonthlyReturnRecord[] { stockWithHighestInvestmentValue }))
            {
                investmentResult.Add(new CsvWriteRecord(stockToInvest)
                {
                    PurchaseQty = (int)Math.Round(stockWithHighestInvestmentValue.Open / stockToInvest.Open)
                });
            }
        }

        private static void ProcessStocksForDisinvestment(IGrouping<dynamic, CompanyMonthlyReturnRecord> month, List<CsvWriteRecord> investmentResult)
        {
            var stocksToDisinvest = month.Where(q => q.Return < 0)
                                        .OrderBy(o => o.Return)
                                        .Take(NoOfStocksToInvest);

            foreach (var stockToDisinvest in stocksToDisinvest)
            {
                ProcessInvestmentsForSellForASpecificStock(month.Key.Year,
                                                                month.Key.Month,
                                                                month.FirstOrDefault(q => q.Symbol == stockToDisinvest.Symbol)?.Close ?? 0,
                                                                investmentResult,
                                                                stockToDisinvest);
            }
        }

        private static void ProcessStocksForFinalSaleReturn(List<CompanyMonthlyReturnRecord> monthlyReturnRecords, List<CsvWriteRecord> investmentResult)
        {
            var symbolWiseMostRecentMonths = monthlyReturnRecords.GroupBy(g => g.Symbol)
                                                                .Select(s => new
                                                                {
                                                                    Symbol = s.Key,
                                                                    Date = s.Max(q => new DateTime(q.Year, q.Month, 1))
                                                                });
            foreach (var symbolMonth in symbolWiseMostRecentMonths)
            {
                var recentPriceOfStock = monthlyReturnRecords
                                                    .FirstOrDefault(q => q.Symbol == symbolMonth.Symbol
                                                                        && q.Year == symbolMonth.Date.Year
                                                                        && q.Month == symbolMonth.Date.Month);
                ProcessInvestmentsForSellForASpecificStock(recentPriceOfStock.Year,
                                                            recentPriceOfStock.Month,
                                                            recentPriceOfStock.Close,
                                                            investmentResult,
                                                            recentPriceOfStock);
            }

        }

        private static void ProcessInvestmentsForSellForASpecificStock(int year,
                                                                        int month,
                                                                        decimal sellPrice,
                                                                        List<CsvWriteRecord> investmentResult,
                                                                        CompanyMonthlyReturnRecord stockToDisinvest)
        {
            var remainingQty = investmentResult
                                .Where(q => q.Symbol == stockToDisinvest.Symbol && q.RemainingQty > 0)
                                .Sum(s => s.RemainingQty);
            if (remainingQty <= 0)
            {
                return;
            }

            foreach (var stock in investmentResult
                                        .Where(q => q.Symbol == stockToDisinvest.Symbol
                                                    && q.RemainingQty > 0))
            {
                stock.SellDate = new DateTime(year, month, 1).AddMonths(1);
                stock.SellPrice = sellPrice;
                int quyToSell = Math.Min(remainingQty, stock.RemainingQty);
                stock.SellQty += quyToSell;
                remainingQty -= quyToSell;
                if (remainingQty <= 0)
                {
                    break;
                }
            }
        }
    }
}
