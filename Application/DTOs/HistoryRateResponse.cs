using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class HistoryRateResponse
    {
        public string Result { get; set; }
        public string BaseCode { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public Dictionary<string, decimal> ConversionRates { get; set; }
        public string ErrorType { get; set; }
    }
}

