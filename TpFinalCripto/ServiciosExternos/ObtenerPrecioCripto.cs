using TpFinalCripto.Models.Externos;  // Importamos el modelo para deserializar la respuesta externa de la API CriptoYa
using System.Net.Http.Json;
using System.Text.Json;             // Para usar métodos de HttpClient que manejan JSON automáticamente

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
            var client = _httpClientFactory.CreateClient();
            string url = $"https://criptoya.com/api/satoshitango/{cryptoCode}/ars";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error al consultar la API: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var json = System.Text.Json.JsonDocument.Parse(content);
                if (!json.RootElement.TryGetProperty("ask", out var askElement))
                {
                    throw new Exception("El campo 'ask' no se encuentra en la respuesta.");
                }

                return askElement.GetDecimal();
            }
            catch (Exception ex)
            {
                throw new Exception($"Respuesta no es JSON válido: {ex.Message}");
            }
        }
    }
}
