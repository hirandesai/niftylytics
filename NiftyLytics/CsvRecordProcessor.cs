using AlphaVantage;
using AlphaVantage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NiftyLytics
{
    public class CsvRecordProcessor
    {
        private readonly AlphaVantageClient alphaVantageClient;
        public CsvRecordProcessor(AlphaVantageClient alphaVantageClient)
        {
            this.alphaVantageClient = alphaVantageClient;
        }

        public async Task<List<CsvWriteRecord>> ProcessCSVRecords(IEnumerable<CsvReadRecord> csvRecords)
        {
            if (!csvRecords.Any())
            {
                return null;
            }
            var processedRecords = new List<CsvWriteRecord>();
            foreach (var record in csvRecords)
            {
                var processedRecord = new CsvWriteRecord(record);
                if (string.IsNullOrEmpty(record.SymbolCode))
                {
                    continue;
                }
                var response = await alphaVantageClient.GetMonthlyAdjustedPricesForSymbol(record.SymbolCode);
                MonthlyAdjustedTimeSeriesRecord currentMonthRecord = GetCurrentMonthRecord(response.MonthlyAdjustedTimeSeries),
                                                previousMonthRecord = GetRecord(response.MonthlyAdjustedTimeSeries, GetPreviousMonthDate()),
                                                sixMonthRecord = GetRecord(response.MonthlyAdjustedTimeSeries, GetSixMonthDate()),
                                                oneYearMonthRecord = GetRecord(response.MonthlyAdjustedTimeSeries, GetOneYearDate()),
                                                threeYearMonthRecord = GetRecord(response.MonthlyAdjustedTimeSeries, GetThreeYearDate()),
                                                fiveYearMonthRecord = GetRecord(response.MonthlyAdjustedTimeSeries, GetFiveYearDate());

                processedRecord.RecentPrice = currentMonthRecord.Close;
                processedRecord.OneMonthReturn = CalculateReturnPrice(currentMonthRecord,previousMonthRecord);
                processedRecord.SixMonthReturn = CalculateReturnPrice(currentMonthRecord, sixMonthRecord);
                processedRecord.OneYearReturn = CalculateReturnPrice(currentMonthRecord, oneYearMonthRecord);
                processedRecord.ThreeYearReturn = CalculateReturnPrice(currentMonthRecord, threeYearMonthRecord);
                processedRecord.FiveYearReturn = CalculateReturnPrice(currentMonthRecord, fiveYearMonthRecord);
                processedRecords.Add(processedRecord);
            }
            return processedRecords;
        }

        private MonthlyAdjustedTimeSeriesRecord GetCurrentMonthRecord(Dictionary<DateTime, MonthlyAdjustedTimeSeriesRecord> monthlyAdjustedTimeSeries)
        {
            if (monthlyAdjustedTimeSeries == null)
            {
                return null;
            }
            var getCurrentMonthDate = GetCurrentMonthDate();
            if (!monthlyAdjustedTimeSeries.TryGetValue(getCurrentMonthDate, out MonthlyAdjustedTimeSeriesRecord record))
            {
                return monthlyAdjustedTimeSeries.Values.FirstOrDefault();
            }
            return record;
        }
        
        private DateTime GetCurrentMonthDate()
        {
            var currentDate = DateTime.Now.Date;
            if (currentDate.Day < 20)
            {
                currentDate = currentDate.AddMonths(-1);
                currentDate = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
            }
            return currentDate;
        }

        private MonthlyAdjustedTimeSeriesRecord GetRecord(Dictionary<DateTime, MonthlyAdjustedTimeSeriesRecord> monthlyAdjustedTimeSeries, DateTime dateTime)
        {
            MonthlyAdjustedTimeSeriesRecord record;
            do
            {
                monthlyAdjustedTimeSeries.TryGetValue(dateTime, out record);
                dateTime = dateTime.AddDays(-1);
            } while (record == null && dateTime.Day != 1);
            return record;
        }

        private DateTime GetPreviousMonthDate()
        {
            var previousMonthDate = GetCurrentMonthDate().AddMonths(-1);
            return new DateTime(previousMonthDate.Year, previousMonthDate.Month, DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month));
        }
        
        private DateTime GetSixMonthDate()
        {
            var previousMonthDate = GetCurrentMonthDate().AddMonths(-6);
            return new DateTime(previousMonthDate.Year, previousMonthDate.Month, DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month));
        }
        
        private DateTime GetOneYearDate()
        {
            var previousMonthDate = GetCurrentMonthDate().AddYears(-1);
            return new DateTime(previousMonthDate.Year, previousMonthDate.Month, DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month));
        }

        private DateTime GetThreeYearDate()
        {
            var previousMonthDate = GetCurrentMonthDate().AddYears(-3);
            return new DateTime(previousMonthDate.Year, previousMonthDate.Month, DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month));
        }

        private DateTime GetFiveYearDate()
        {
            var previousMonthDate = GetCurrentMonthDate().AddYears(-5);
            return new DateTime(previousMonthDate.Year, previousMonthDate.Month, DateTime.DaysInMonth(previousMonthDate.Year, previousMonthDate.Month));
        }

        private decimal CalculateReturnPrice(MonthlyAdjustedTimeSeriesRecord close, MonthlyAdjustedTimeSeriesRecord open)
        {
            if (close == null || open == null)
            {
                return 0;
            }
            return Math.Round(((close.Close - open.Open) / open.Open) * 100, 2);
        }
    }
}
