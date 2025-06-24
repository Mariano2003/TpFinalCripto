using TpFinalCripto.Models.Externos;  // Importamos el modelo para deserializar la respuesta externa de la API CriptoYa
using System.Net.Http.Json;             // Para usar métodos de HttpClient que manejan JSON automáticamente

namespace TpFinalCripto.ServiciosExternos
{
    // Definimos una interfaz para el servicio que nos proveerá el precio de la criptomoneda
    // Así podemos cambiar la implementación sin romper el resto del código. ¡Separación de responsabilidades FTW!
    public interface IServicioPrecioDeCripto
    {
        Task<decimal> ObtenerPrecioCripto(string cryptoCode); // Método asíncrono que devuelve el precio de la cripto
    }

    // Implementación concreta del servicio que se conecta a la API externa CriptoYa para obtener precios
    public class ServicioPrecioDeCripto : IServicioPrecioDeCripto
    {
        private readonly IHttpClientFactory _httpClientFactory;  // Inyectamos un factory para crear HttpClient y no andar creando clientes a lo loco

        // Constructor que recibe el factory para crear clientes HTTP (inyección de dependencias, no es brujería)
        public ServicioPrecioDeCripto(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Método que consulta la API externa y devuelve el precio actual de la cripto especificada
        public async Task<decimal> ObtenerPrecioCripto(string cryptoCode)
        {
            var client = _httpClientFactory.CreateClient(); // Creamos un cliente HTTP listo para usarse
            string url = $"https://criptoya.com/api/satoshitango/{cryptoCode}/ars"; // Armamos la URL con la cripto y la moneda ARS

            // Hacemos la llamada GET a la API y deserializamos automáticamente la respuesta JSON en nuestro modelo CriptoYaResponse
            var response = await client.GetFromJsonAsync<CriptoYaResponse>(url);

            // Si la respuesta fue nula, tiramos un error para que quien llame sepa que algo salió mal
            if (response == null)
                throw new Exception("Respuesta nula de la API");

            // Retornamos el valor 'ask' (precio para comprar) que viene en la respuesta JSON
            return response.ask;
        }
    }
}
