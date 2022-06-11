namespace NiftyNext50.Models
{
    public class CsvWriteRecord : CompanyMonthlyReturnRecord
    {
        public CsvWriteRecord(CompanyMonthlyReturnRecord record) : base(record)
        {
            base.Close = record.Close;
            base.Open = record.Open;
            base.Year = record.Year;
            base.Month = record.Month;
        }

        public int Qty { get; set; }
        public decimal Investment
        {
            get
            {
                return Qty * Open;
            }
        }

    }
}
