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
#if DEBUG
            var filePath = @"C:\Users\hiran.desai\Downloads\ind_niftynext50list.csv";
#else

            var filePath = GetFilePathFromUser();
#endif

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            using (var priceCardStreamWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, $"price-card-{Guid.NewGuid()}.csv")))
            using (var priceCardCsvWriter = new CsvWriter(priceCardStreamWriter, CultureInfo.InvariantCulture))
            {
                using (var investmentDetailStreamWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, $"investment-details-{Guid.NewGuid()}.csv")))
                using (var investmentDetailCsvWriter = new CsvWriter(investmentDetailStreamWriter, CultureInfo.InvariantCulture))
                {
                    var recordProcessor = new CsvRecordProcessor(new AlphaVantageClient("XICHYWYADQ0KKG4G"), priceCardCsvWriter, investmentDetailCsvWriter);
                    using (var reader = new StreamReader(filePath))
                    using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csvReader.Context.RegisterClassMap<CsvReadRecordMap>();
                        var records = csvReader.GetRecords<CsvReadRecord>().ToList();
                        await recordProcessor.ProcessCSVRecords(records);
                    }
                }
            }
            
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
