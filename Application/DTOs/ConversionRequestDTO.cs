using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record ConversionRequestDTO(
    [property: Required, StringLength(3, MinimumLength = 3)] 
    string FromCurrency,
    [property: Required, StringLength(3, MinimumLength = 3)] 
    string ToCurrency,
    [property: Range(0.01, double.MaxValue)] 
    decimal Amount,
    [property: Required] 
    DateTime Date);

}
