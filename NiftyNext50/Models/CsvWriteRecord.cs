using CsvHelper.Configuration.Attributes;
using System;

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
            PurchasePrice = record.Open;
        }

        [Format("dd/MM/yyyy")]
        public DateTime PurchaseDate => new DateTime(Year, Month, 1).AddMonths(1);

        public decimal PurchasePrice { get; private set; }

        public int PurchaseQty { get; set; }

        public decimal PurchaseAmount
        {
            get
            {
                return PurchaseQty * PurchasePrice;
            }
        }

        [Format("dd/MM/yyyy")]
        public DateTime SellDate { get; set; }

        public decimal SellPrice { get; set; }

        public int SellQty { get; set; }

        public decimal SellAmount
        {
            get
            {
                return SellQty * SellPrice;
            }
        }

        public decimal Profit
        {
            get
            {
                return SellAmount - PurchaseAmount;
            }
        }

        public decimal AbsoluteReturn
        {
            get
            {
                return (Profit / PurchaseAmount) * 100;
            }
        }

        public decimal YearlyReturn
        {
            get
            {
                if ((SellDate - PurchaseDate).TotalDays == 0)
                {
                    return AbsoluteReturn;
                }
                return AbsoluteReturn / (decimal)((SellDate - PurchaseDate).TotalDays / 365);
            }
        }

        public int RemainingQty => PurchaseQty - SellQty;
    }
}
