using System;

namespace NiftyNext50.Models
{
    public class CompanyMonthlyReturnRecord : CsvReadRecord
    {
        public CompanyMonthlyReturnRecord(CsvReadRecord record)
        {
            base.CompanyName = record.CompanyName;
            base.Industry = record.Industry;
            base.Symbol = record.Symbol;
        }
        public decimal Close { get; set; }
        public decimal Open { get; set; }
        public decimal Return
        {
            get
            {
                if (Open == 0)
                {
                    return -1;
                }
                return (Close - Open) / Open;
            }
        }
        public int Year { get; set; }
        public int Month { get; set; }

    }
}
