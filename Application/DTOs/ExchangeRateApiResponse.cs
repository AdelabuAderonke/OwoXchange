using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ExchangeRateApiResponse
    {
        public string Result { get; set; }
        public string Documentation { get; set; }
        public string Terms_of_use { get; set; }
        public long Time_last_update_unix { get; set; }
        public string Time_last_update_utc { get; set; }
        public long Time_next_update_unix { get; set; }
        public string Time_next_update_utc { get; set; }
        public string Base_code { get; set; }
        public string Target_code { get; set; }
        public string ErrorType { get; set; }
        public decimal Conversion_rate { get; set; }
    }
}
