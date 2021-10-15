namespace NiftyLytics
{
    public class CsvWriteRecord : CsvReadRecord
    {
        public CsvWriteRecord(CsvReadRecord record)
        {
            base.Stock = record.Stock;
            base.Segment = record.Segment;
            base.Weightage = record.Weightage;
            base.SymbolCode = record.SymbolCode;
        }
        public decimal? RecentPrice { get; set; }

        public decimal? OneMonthReturn { get; set; }

        public decimal? SixMonthReturn { get; set; }

        public decimal? OneYearReturn { get; set; }

        public decimal? ThreeYearReturn { get; set; }

        public decimal? FiveYearReturn { get; set; }
    }
}
