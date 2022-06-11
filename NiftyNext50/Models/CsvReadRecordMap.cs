using CsvHelper.Configuration;

namespace NiftyNext50.Models
{
    public sealed class CsvReadRecordMap : ClassMap<CsvReadRecord>
    {
        public CsvReadRecordMap()
        {
            Map(m => m.CompanyName).Name("Company Name");
            Map(m => m.Industry).Name("Industry");
            Map(m => m.Symbol).Name("Symbol");
        }
    }
}
