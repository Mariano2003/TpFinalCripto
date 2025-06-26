using System;
using System.ComponentModel.DataAnnotations;

namespace TpFinalCripto.DTOs
{
    public class TransaccionUpdateDto
    {
        [Range(0.00001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
        public decimal? CryptoAmount { get; set; }

        public DateTime? FechaHora { get; set; }

        public decimal? Money { get; set; }
    }
}
