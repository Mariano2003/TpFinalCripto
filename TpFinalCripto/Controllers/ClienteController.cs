using TpFinalCripto.DTOs;        // Importa los DTOs para manejar las entradas y salidas
using TpFinalCripto.Models;      // Importa los modelos de datos de la base
using Microsoft.AspNetCore.Mvc;  // Importa funcionalidades para controladores web y API
using Microsoft.EntityFrameworkCore; // Importa EF Core para acceso a base de datos async

namespace TpFinalCripto.Controllers
{
    [Route("api/[controller]")]  // Define la ruta base para este controlador: /api/clientes
    [ApiController]               // Indica que es un controlador API (maneja automáticamente modelos y validaciones)
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;  // Contexto de base de datos para hacer queries

        // Constructor que recibe el contexto inyectado (Dependency Injection)
        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/clientes
        [HttpGet]  // Método GET para obtener todos los clientes
        public async Task<ActionResult<IEnumerable<ClienteReadDto>>> Get()
        {
            // Consulta async para traer todos los clientes de la DB
            var clientes = await _context.Clientes.ToListAsync();

            // Mapea cada cliente a su DTO de salida (read)
            var clienteDtos = clientes.Select(c => new ClienteReadDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email
            }).ToList();

            // Devuelve HTTP 200 OK con la lista de clientes DTO
            return Ok(clienteDtos);
        }

        // GET: api/clientes/5
        [HttpGet("{id}")] // Método GET para obtener un cliente específico por id
        public async Task<ActionResult<ClienteReadDto>> Get(int id)
        {
            // Busca el cliente por id en la DB
            var cliente = await _context.Clientes.FindAsync(id);

            // Si no lo encuentra, devuelve 404 Not Found
            if (cliente == null)
                return NotFound();

            // Mapea el cliente encontrado a DTO de salida
            var dto = new ClienteReadDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Email = cliente.Email
            };

            // Devuelve 200 OK con el cliente DTO
            return Ok(dto);
        }

        // POST: api/clientes
        [HttpPost] // Método POST para crear un nuevo cliente
        public async Task<ActionResult<ClienteReadDto>> Post(ClienteCreateDto dto)
        {
            // Crea un nuevo objeto Cliente a partir del DTO recibido
            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                Email = dto.Email
            };

            // Agrega el nuevo cliente al contexto para ser insertado
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync(); // Guarda los cambios en la base (inserción real)

            // Crea un DTO de salida con el cliente ya creado (incluye Id generado)
            var clienteDto = new ClienteReadDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Email = cliente.Email
            };

            // Devuelve HTTP 201 Created con la ubicación y el nuevo recurso
            return CreatedAtAction(nameof(Get), new { id = cliente.Id }, clienteDto);
        }

        // PUT: api/clientes/5
        [HttpPut("{id}")] // Método PUT para actualizar un cliente existente
        public async Task<IActionResult> Put(int id, ClienteUpdateDto dto)
        {
            // Busca el cliente en la base por id
            var cliente = await _context.Clientes.FindAsync(id);

            // Si no existe, devuelve 404 Not Found
            if (cliente == null)
                return NotFound();

            // Actualiza las propiedades del cliente con los datos del DTO recibido
            cliente.Nombre = dto.Nombre;
            cliente.Email = dto.Email;

            // Guarda los cambios en la base (update real)
            await _context.SaveChangesAsync();

            // Devuelve HTTP 204 No Content porque la operación fue exitosa sin cuerpo
            return NoContent();
        }

        // DELETE: api/clientes/5
        [HttpDelete("{id}")] // Método DELETE para eliminar un cliente por id
        public async Task<IActionResult> Delete(int id)
        {
            // Busca el cliente en la base por id
            var cliente = await _context.Clientes.FindAsync(id);

            // Si no existe, devuelve 404 Not Found
            if (cliente == null)
                return NotFound();

            // Elimina el cliente del contexto y guarda cambios (delete real)
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            // Devuelve HTTP 204 No Content para indicar que se borró con éxito
            return NoContent();
        }
    }
}