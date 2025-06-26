using System.ComponentModel.DataAnnotations;

namespace TpFinalCripto.DTOs
{
    public class TransaccionCreateDto
    {
        [Required]
        [RegularExpression("purchase|sale", ErrorMessage = "La acción debe ser 'purchase' o 'sale'.")]
        public string Action { get; set; }

        [Required]
        [Range(0.00001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public decimal CryptoAmount { get; set; }

        [Required]
      
        public string CryptoCode { get; set; }

        [Required]
  
        public DateTime FechaHora { get; set; }

        [Required]
        public int ClienteId { get; set; }
    }
}
