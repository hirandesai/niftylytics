using CsvHelper;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NiftyLytics
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ProcessCsv();
        }

        private static async Task ProcessCsv()
        {
            //var filePath = GetFilePathFromUser();
            var filePath = @"C:\Users\hiran.desai\Downloads\Middle_Class_Investment_SmallCase.csv";
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            CsvRecordProcessor recordProcessor = new CsvRecordProcessor(new AlphaVantageClient("XICHYWYADQ0KKG4G"));
            using (var reader = new StreamReader(filePath))
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csvReader.Context.RegisterClassMap<CsvReadRecordMap>();
                var records = csvReader.GetRecords<CsvReadRecord>().ToList();
                var processedRecords = await recordProcessor.ProcessCSVRecords(records);

                using (var writer = new StreamWriter(Path.Combine(Environment.CurrentDirectory, $"smallcase-{Guid.NewGuid()}.csv")))
                using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecords(processedRecords);
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
