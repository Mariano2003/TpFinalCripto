using TpFinalCripto.DTOs;
using TpFinalCripto.Models;
using Microsoft.EntityFrameworkCore;

namespace TpFinalCripto.Validaciones
{
    public static class ValidacionTransaccion
    {
        // Lista ampliada para aceptar códigos y nombres comunes
        private static readonly HashSet<string> CriptosValidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "btc",
            "eth",
            "usdt"
        };

        public static bool EsCryptoValida(string cryptoCode) =>
            CriptosValidas.Contains(cryptoCode);


        public static bool EsAccionValida(string accion) =>
            accion == "purchase" || accion == "sale";

    

        public static bool EsCantidadValida(decimal cantidad) =>
            cantidad > 0;

        public static bool EsFechaValida(DateTime fecha) =>
            fecha <= DateTime.UtcNow;  // No permite fechas futuras

        public static async Task<bool> TieneSaldoSuficiente(AppDbContext context, TransaccionCreateDto dto)
        {
            if (dto.Action != "sale") return true;

            var totalComprado = await context.Transacciones
                .Where(t => t.ClienteId == dto.ClienteId && t.CryptoCode == dto.CryptoCode.ToLower() && t.Action == "purchase")
                .SumAsync(t => t.CryptoAmount);

            var totalVendido = await context.Transacciones
                .Where(t => t.ClienteId == dto.ClienteId && t.CryptoCode == dto.CryptoCode.ToLower() && t.Action == "sale")
                .SumAsync(t => t.CryptoAmount);

            var saldoActual = totalComprado - totalVendido;

            return dto.CryptoAmount <= saldoActual;
        }
    }
}
