namespace TpFinalCripto.DTOs
{
    public class TransaccionReadDto
    {
        public int Id { get; set; }
        public string CryptoCode { get; set; }
        public decimal CryptoAmount { get; set; }
        public int ClienteId { get; set; }
        public string Action { get; set; }
        public decimal Money { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
