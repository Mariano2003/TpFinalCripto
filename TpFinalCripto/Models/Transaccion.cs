using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TpFinalCripto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
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

        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; }
    }
}
