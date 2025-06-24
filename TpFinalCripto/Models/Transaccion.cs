using System.ComponentModel.DataAnnotations;

namespace TpFinalCripto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }

        [Required]
        public string CryptoCode { get; set; }

        [Required]
        public decimal CryptoAmount { get; set; }

        [Required]
        public decimal Money { get; set; }

        [Required]
        [RegularExpression("purchase|sale")]
        public string Action { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        public int ClientId { get; set; }
        public Cliente Cliente { get; set; }
    }
}
