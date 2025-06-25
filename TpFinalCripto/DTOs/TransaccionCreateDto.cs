using System.ComponentModel.DataAnnotations;

namespace TpFinalCripto.DTOs
{
    public class TransaccionCreateDto
    {
        [Required]
        public string CryptoCode { get; set; }  // Ej: "bitcoin", "usdc", "eth"

        [Required]
        [Range(0.00001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
        public decimal CryptoAmount { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        [RegularExpression("purchase|sale", ErrorMessage = "Action debe ser 'purchase' o 'sale'")]
        public string Action { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }
    }
}
