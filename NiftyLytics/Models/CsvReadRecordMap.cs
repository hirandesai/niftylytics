using CsvHelper.Configuration;

namespace NiftyLytics
{
    public sealed class CsvReadRecordMap : ClassMap<CsvReadRecord>
    {
        public CsvReadRecordMap()
        {
            Map(m => m.Stock).Name("Stock");
            Map(m => m.Segment).Name("Segment", "Market Segment");
            Map(m => m.Weightage).Name("Weightage", "Weightage (%)");
            Map(m => m.SymbolCode).Name("Symbol Code", "SymbolCode");
        }
    }
}
