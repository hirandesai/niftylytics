using AlphaVantage;
using CsvHelper;
using NiftyNext50.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NiftyNext50
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ProcessCsv();
            Console.ReadLine();
        }

        private static async Task ProcessCsv()
        {
            // get the csv from https://archives.nseindia.com/content/indices/ind_niftynext50list.csv
            // reference page - https://www.nseindia.com/products-services/indices-niftynext50-index
#if DEBUG
            var filePath = @"C:\Users\desaihi\Downloads\ind_nifty500list.csv";
#else

            var filePath = GetFilePathFromUser();
#endif

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            var investmentDetaiilsFileName = $"investment-details-{Guid.NewGuid()}.csv";
            var priceCardFileName = $"price-card-{Guid.NewGuid()}.csv";
            using (var priceCardStreamWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, priceCardFileName)))
            using (var priceCardCsvWriter = new CsvWriter(priceCardStreamWriter, CultureInfo.InvariantCulture))
            {
                using (var investmentDetailStreamWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, investmentDetaiilsFileName)))
                using (var investmentDetailCsvWriter = new CsvWriter(investmentDetailStreamWriter, CultureInfo.InvariantCulture))
                {
                    var recordProcessor = new CsvRecordProcessor(new AlphaVantageClient("XICHYWYADQ0KKG4G"), priceCardCsvWriter, investmentDetailCsvWriter);
                    using (var reader = new StreamReader(filePath))
                    using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csvReader.Context.RegisterClassMap<CsvReadRecordMap>();
                        var records = csvReader.GetRecords<CsvReadRecord>().ToList();

                        // Keep `considerSell` to true if you want to get sell recommendation. Keeping it to false will only give you
                        // purchase recommendations.
                        // Keep `shouldSellEveryThingToday` to false if you don't want to sell everything which was purchase previously.
                        // Keep `shouldSellEveryThingToday` to true if you want to sell everything.  Ideally this should be done
                        // when calculating the returns on specific day.
                        await recordProcessor.ProcessCsvRecords(records, considerSell: true, shouldSellEveryThingToday: false);
                    }
                }
            }
            Console.WriteLine($"Price card file generated successfully :: {priceCardFileName}");
            Console.WriteLine($"Investment file generated successfully :: {investmentDetaiilsFileName}");
        }

        private static string GetFilePathFromUser()
        {
            string filePath = null;
            do
            {
                Console.WriteLine("Provide CSV Path or type Exit (to close)!");
                filePath = Console.ReadLine();
                if (filePath.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                {
                    filePath = null;
                    break;
                }
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File does not exists at path {filePath}!");
                }
            } while (!File.Exists(filePath));
            return filePath;
        }
    }
}
