using TpFinalCripto.DTOs;   // Importa los DTOs para usar en las validaciones (ej: TransaccionCreateDto)
using TpFinalCripto.Models; // Importa los modelos (ej: Transaccion)
using Microsoft.EntityFrameworkCore; // Para usar métodos asíncronos y consultas LINQ a la BD

namespace TpFinalCripto.Validaciones
{
    public class ValidacionTransaccion
    {
        // Método estático para validar que la acción sea válida, o sea "purchase" o "sale"
        // ¿Qué más podría ser? "bailar" no es una acción válida en este contexto.
        public static bool EsAccionValida(string accion) =>
            accion == "purchase" || accion == "sale";

        // Método asíncrono para validar si el cliente tiene saldo suficiente para vender
        public static async Task<bool> TieneSaldoSuficiente(AppDbContext context, TransaccionCreateDto dto)
        {
            // Si la acción no es "sale" (venta), no hace falta chequear saldo porque compra no tiene límite acá
            if (dto.Action != "sale") return true;

            // Calculamos cuánto compró el cliente de esta criptomoneda (suma total de compras)
            var totalComprado = await context.Transacciones
                .Where(t => t.ClientId == dto.ClientId && t.CryptoCode == dto.CryptoCode && t.Action == "purchase")
                .SumAsync(t => t.CryptoAmount);

            // Calculamos cuánto vendió el cliente de esta criptomoneda (suma total de ventas)
            var totalVendido = await context.Transacciones
                .Where(t => t.ClientId == dto.ClientId && t.CryptoCode == dto.CryptoCode && t.Action == "sale")
                .SumAsync(t => t.CryptoAmount);

            // Saldo actual es la diferencia entre lo comprado y lo vendido
            var saldoActual = totalComprado - totalVendido;

            // Retornamos true si la cantidad que quiere vender es menor o igual al saldo disponible
            // Si no, false (porque no tiene saldo suficiente, y no queremos que regale cripto gratis)
            return dto.CryptoAmount <= saldoActual;
        }
    }

}

