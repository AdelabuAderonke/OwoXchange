using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public  class ExchangeRate
    {
        public int Id {  get; set; }
        public string BasecurrencyCode {  get; set; }
        public string TargetcurrencyCode { get; set; }
        public decimal Rate {  get; set; }
        public DateTime Date { get; set; }
        public bool IsHistorical { get; set; }
    }
}
