﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record ExchangeRateDTO(
    
        string BaseCurrency,
        string TargetCurrency,
        decimal Rate,
        string FormattedDate);
    
}
