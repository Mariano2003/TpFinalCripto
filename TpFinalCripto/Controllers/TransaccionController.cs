using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpFinalCripto.DTOs;
using TpFinalCripto.Models;
using TpFinalCripto.ServiciosExternos;
using TpFinalCripto.Validaciones;

namespace TpFinalCripto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransaccionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IServicioPrecioDeCripto _criptoPriceService;

        public TransaccionController(AppDbContext context, IServicioPrecioDeCripto criptoPriceService)
        {
            _context = context;
            _criptoPriceService = criptoPriceService;
        }

        // POST: api/transaccion
        [HttpPost]
        public async Task<ActionResult<TransaccionReadDto>> Post(TransaccionCreateDto dto)
        {
            // Validaciones
            if (!ValidacionTransaccion.EsCantidadValida(dto.CryptoAmount))
                return BadRequest("La cantidad de criptomonedas debe ser mayor a 0.");

            if (!ValidacionTransaccion.EsAccionValida(dto.Action))
                return BadRequest("La acción debe ser 'purchase' o 'sale'.");

            if (!ValidacionTransaccion.EsCryptoValida(dto.CryptoCode))
                return BadRequest($"La criptomoneda '{dto.CryptoCode}' no es soportada.");

            if (!ValidacionTransaccion.EsFechaValida(dto.FechaHora))
                return BadRequest("La fecha no puede ser futura.");

            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return BadRequest("Cliente no encontrado.");

            if (!await ValidacionTransaccion.TieneSaldoSuficiente(_context, dto))
                return BadRequest("Saldo insuficiente para realizar la venta.");

            decimal precioUnitario;
            try
            {
                precioUnitario = await _criptoPriceService.ObtenerPrecioCripto(dto.CryptoCode);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener precio de la criptomoneda: {ex.Message}");
            }

            var montoTotal = precioUnitario * dto.CryptoAmount;

            var transaccion = new Transaccion
            {
                CryptoCode = dto.CryptoCode.ToLower(),
                Action = dto.Action,
                ClienteId = dto.ClienteId,
                CryptoAmount = dto.CryptoAmount,
                Money = montoTotal,
                FechaHora = dto.FechaHora
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

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

            return CreatedAtAction(nameof(GetById), new { id = transaccion.Id }, transaccionReadDto);
        }

        // GET: api/transaccion
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransaccionReadDto>>> GetAll()
        {
            var transacciones = await _context.Transacciones.ToListAsync();

            var result = transacciones.Select(t => new TransaccionReadDto
            {
                Id = t.Id,
                CryptoCode = t.CryptoCode,
                Action = t.Action,
                ClienteId = t.ClienteId,
                CryptoAmount = t.CryptoAmount,
                Money = t.Money,
                FechaHora = t.FechaHora
            }).OrderByDescending(t => t.FechaHora).ToList();

            return Ok(result);
        }

        // GET: api/transaccion/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TransaccionReadDto>> GetById(int id)
        {
            var transaccion = await _context.Transacciones.FindAsync(id);
            if (transaccion == null)
                return NotFound();

            var dto = new TransaccionReadDto
            {
                Id = transaccion.Id,
                CryptoCode = transaccion.CryptoCode,
                Action = transaccion.Action,
                ClienteId = transaccion.ClienteId,
                CryptoAmount = transaccion.CryptoAmount,
                Money = transaccion.Money,
                FechaHora = transaccion.FechaHora
            };

            return Ok(dto);
        }

        // PATCH: api/transaccion/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, TransaccionUpdateDto dto)
        {
            var transaccion = await _context.Transacciones.FindAsync(id);
            if (transaccion == null)
                return NotFound();

            // Actualizamos CryptoAmount si viene y validamos
            if (dto.CryptoAmount.HasValue)
            {
                if (!ValidacionTransaccion.EsCantidadValida(dto.CryptoAmount.Value))
                    return BadRequest("La cantidad debe ser mayor a 0.");

                transaccion.CryptoAmount = dto.CryptoAmount.Value;

                // Recalculamos Money con el precio actual de la cripto
                try
                {
                    var precioUnitario = await _criptoPriceService.ObtenerPrecioCripto(transaccion.CryptoCode);
                    transaccion.Money = precioUnitario * transaccion.CryptoAmount;
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error al obtener precio de la criptomoneda: {ex.Message}");
                }
            }

            // Actualizamos FechaHora si viene y validamos
            if (dto.FechaHora.HasValue)
            {
                if (!ValidacionTransaccion.EsFechaValida(dto.FechaHora.Value))
                    return BadRequest("La fecha no puede ser futura.");
                transaccion.FechaHora = dto.FechaHora.Value;
            }

            // Money no se debe actualizar directamente si CryptoAmount cambia,
            // por eso ignoramos dto.Money en ese caso.
            // Si viene y no se cambió CryptoAmount, actualizamos solo si no es nulo.
            if (dto.Money.HasValue && !dto.CryptoAmount.HasValue)
            {
                transaccion.Money = dto.Money.Value;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/transaccion/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaccion = await _context.Transacciones.FindAsync(id);
            if (transaccion == null)
                return NotFound();

            _context.Transacciones.Remove(transaccion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
