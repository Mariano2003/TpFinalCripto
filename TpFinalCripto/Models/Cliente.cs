using System.ComponentModel.DataAnnotations;


namespace TpFinalCripto.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public ICollection<Transaccion> Transacciones { get; set; } = new List <Transaccion>();
    }
}
