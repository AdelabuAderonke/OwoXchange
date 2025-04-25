using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record ConversionResultDTO(
    
      string BasecurrencyCode ,
      string TargetcurrencyCode,
      decimal Rate,
      decimal ConvertedAmount,
       DateTime ConversionDate);
}
