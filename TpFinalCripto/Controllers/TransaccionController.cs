using Microsoft.AspNetCore.Mvc;  // Herramientas para controladores web en ASP.NET Core
using TpFinalCripto.DTOs;          // DTOs (capas intermedias de datos)
using TpFinalCripto.Models;        // Modelos que representan tablas de la BD
using TpFinalCripto.ServiciosExternos; // Servicio para obtener precios de criptos desde API externa
using TpFinalCripto.Validaciones;  // Validaciones personalizadas para transacciones

namespace TpFinalCripto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransaccionController : ControllerBase
    {
        private readonly AppDbContext _context;  // Contexto para BD
        private readonly IServicioPrecioDeCripto _criptoPriceService;  // Servicio para precio cripto

        // Constructor con inyección de dependencias
        public TransaccionController(AppDbContext context, IServicioPrecioDeCripto criptoPriceService)
        {
            _context = context;
            _criptoPriceService = criptoPriceService;
        }

        // POST: api/transaccion -> Crea una nueva transacción
        [HttpPost]
        public async Task<ActionResult<TransaccionReadDto>> Post(TransaccionCreateDto dto)
        {
            // Validaciones básicas con mensajes claros

            // 1) Cantidad > 0
            if (dto.CryptoAmount <= 0)
                return BadRequest("La cantidad de criptomonedas debe ser mayor a 0.");

            // 2) Acción válida ('purchase' o 'sale')
            if (!ValidacionTransaccion.EsAccionValida(dto.Action))
                return BadRequest("La acción debe ser 'purchase' o 'sale'.");

            // 3) Cliente existe
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return BadRequest("Cliente no encontrado.");

            // 4) Si es venta, validar saldo suficiente con tu método async
            if (!await ValidacionTransaccion.TieneSaldoSuficiente(_context, dto))
                return BadRequest("Saldo insuficiente para realizar la venta.");

            // 5) Obtener precio actual desde servicio externo
            decimal precioUnitario;
            try
            {
                precioUnitario = await _criptoPriceService.ObtenerPrecioCripto(dto.CryptoCode);
            }
            catch (Exception ex)
            {
                // Captura errores del servicio externo y devuelve error 500
                return StatusCode(500, $"Error al obtener precio de la criptomoneda: {ex.Message}");
            }

            // 6) Calcular monto total (precio * cantidad)
            var montoTotal = precioUnitario * dto.CryptoAmount;

            // 7) Crear el objeto transacción para guardar en BD
            var transaccion = new Transaccion
            {
                CryptoCode = dto.CryptoCode.ToLower(),  // uniformizar a minúsculas
                Action = dto.Action,
                ClienteId = dto.ClienteId,
                CryptoAmount = dto.CryptoAmount,
                Money = montoTotal,
                FechaHora = dto.FechaHora
            };

            // 8) Agregar al contexto y guardar cambios
            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            // 9) Preparar DTO para la respuesta (solo campos públicos)
            var transaccionReadDto = new TransaccionReadDto
            {
                Id = transaccion.Id,
                CryptoCode = transaccion.CryptoCode,
                Action = transaccion.Action,
                ClienteId = transaccion.ClienteId,
                CryptoAmount = transaccion.CryptoAmount,
                Money = transaccion.Money,
                FechaHora = transaccion.FechaHora
            };

            // 10) Retornar CreatedAtAction con el resultado y status 201
            return CreatedAtAction(nameof(Post), new { id = transaccion.Id }, transaccionReadDto);
        }
    }
}
