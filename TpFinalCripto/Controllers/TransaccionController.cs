using Microsoft.AspNetCore.Mvc;  // Importa las herramientas para crear controladores web en ASP.NET Core
using TpFinalCripto.DTOs;          // Importa los DTOs (Data Transfer Objects), que son las capas intermedias de datos
using TpFinalCripto.Models;        // Importa las clases modelo que representan las tablas de la base de datos
using TpFinalCripto.ServiciosExternos; // Importa el servicio para obtener precios de criptomonedas de una API externa
using TpFinalCripto.Validaciones;  // Importa las validaciones personalizadas para transacciones

namespace TpFinalCripto.Controllers
{
    [Route("api/[controller]")]  // Define la ruta base para el controlador, que será /api/transaccion
    [ApiController]               // Marca esta clase como un controlador de API (maneja automáticamente muchas cosas como validación y respuestas JSON)
    public class TransaccionController : ControllerBase
    {
        private readonly AppDbContext _context;  // Contexto de base de datos para acceder a las tablas
        private readonly IServicioPrecioDeCripto _criptoPriceService;  // Servicio para obtener el precio de la cripto

        // Constructor para inyección de dependencias: recibe el contexto y el servicio externo
        public TransaccionController(AppDbContext context, IServicioPrecioDeCripto criptoPriceService)
        {
            _context = context;  // Inicializa el contexto para usarlo en todo el controlador
            _criptoPriceService = criptoPriceService;  // Inicializa el servicio para obtener precios de cripto
        }

        // POST: api/transaccion - Aquí se crean nuevas transacciones de criptomonedas
        [HttpPost]
        public async Task<ActionResult<TransaccionReadDto>> Post(TransaccionCreateDto dto)
        {
            // Validación rápida para que no te pase cualquier burrada de cantidad negativa o cero
            if (dto.CryptoAmount <= 0)
                return BadRequest("La cantidad de criptomonedas debe ser mayor a 0.");

            // Validamos que la acción sea 'purchase' o 'sale', no que pongas 'bailar' o 'dormir'
            if (!ValidacionTransaccion.EsAccionValida(dto.Action))
                return BadRequest("La acción debe ser 'purchase' o 'sale'.");

            // Verificamos que el cliente exista, porque transaccionar con fantasmas no es negocio
            var cliente = await _context.Clientes.FindAsync(dto.ClientId);
            if (cliente == null)
                return BadRequest("Cliente no encontrado.");

            // Validamos que si es una venta, el cliente tenga saldo suficiente para no regalar cripto por error
            if (!await ValidacionTransaccion.TieneSaldoSuficiente(_context, dto))
                return BadRequest("Saldo insuficiente para realizar la venta.");

            // Obtenemos el precio actual de la cripto desde el servicio externo, porque no trabajamos con magia ni bolitas de cristal
            decimal precioUnitario;
            try
            {
                precioUnitario = await _criptoPriceService.ObtenerPrecioCripto(dto.CryptoCode);
            }
            catch (Exception ex)
            {
                // Si la API externa falla, aquí decimos que algo salió mal y no que "la culpa es del vecino"
                return StatusCode(500, $"Error al obtener precio de la criptomoneda: {ex.Message}");
            }

            // Calculamos el monto total de la transacción multiplicando la cantidad por el precio
            var montoTotal = precioUnitario * dto.CryptoAmount;

            // Creamos el objeto transacción para guardar en la base de datos
            var transaccion = new Transaccion
            {
                CryptoCode = dto.CryptoCode.ToLower(),  // Convertimos a minúsculas para uniformidad (Bitcoin, bitcoin, BItCoIn... No, todo a "bitcoin")
                Action = dto.Action,
                ClientId = dto.ClientId,
                CryptoAmount = dto.CryptoAmount,
                Money = montoTotal,
                FechaHora = dto.FechaHora  // Fecha y hora de la transacción que se manda desde el cliente
            };

            // Agregamos la transacción al contexto para que EF Core la prepare para guardar
            _context.Transacciones.Add(transaccion);

            // Guardamos efectivamente en la base de datos (aquí es cuando se pone serio)
            await _context.SaveChangesAsync();

            // Preparamos el DTO de respuesta para que el cliente reciba solo lo necesario (sin secretos de la base)
            var transaccionReadDto = new TransaccionReadDto
            {
                Id = transaccion.Id,
                CryptoCode = transaccion.CryptoCode,
                Action = transaccion.Action,
                ClientId = transaccion.ClientId,
                CryptoAmount = transaccion.CryptoAmount,
                Money = transaccion.Money,
                FechaHora = transaccion.FechaHora
            };

            // Retornamos un CreatedAtAction con la ruta al POST (podría ser Get, pero acá se usa Post) y el objeto creado
            return CreatedAtAction(nameof(Post), new { id = transaccion.Id }, transaccionReadDto);
        }
    }
}
