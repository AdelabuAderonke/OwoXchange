using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class ConversionResult
    {
        public int Id {  get; set; }
        public string BasecurrencyCode { get; set; }
        public string TargetcurrencyCode { get; set; }
        public decimal Rate { get; set; }
        public decimal ConvertedAmount {  get; set; }
        public DateTime ConversionDate {  get; set; }
    }
}
