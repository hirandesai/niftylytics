namespace NiftyNext50.Models
{
    public class CsvReadRecord
    {
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public string Symbol { get; set; }
        public string AlphaVantageSymbol
        {
            get
            {
                return $"{Symbol}.BSE";
            }
        }
    }
}