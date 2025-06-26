using TpFinalCripto.Models.Externos;
using System.Net.Http.Json;
using System.Text.Json;

namespace TpFinalCripto.ServiciosExternos
{
    public interface IServicioPrecioDeCripto
    {
        Task<decimal> ObtenerPrecioCripto(string cryptoCode);
    }

    public class ServicioPrecioDeCripto : IServicioPrecioDeCripto
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Lista de criptos válidas mínimas soportadas
        private static readonly HashSet<string> CriptosValidas = new() { "btc", "usdt", "eth" };

        public ServicioPrecioDeCripto(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<decimal> ObtenerPrecioCripto(string cryptoCode)
        {
            if (string.IsNullOrWhiteSpace(cryptoCode))
                throw new ArgumentException("El código de la criptomoneda no puede estar vacío.");

            var cryptoLower = cryptoCode.ToLower();

            if (!CriptosValidas.Contains(cryptoLower))
                throw new ArgumentException($"La criptomoneda '{cryptoCode}' no es soportada.");

            var client = _httpClientFactory.CreateClient();
            string url = $"https://criptoya.com/api/satoshitango/{cryptoLower}/ars";

            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error al consultar la API: {response.StatusCode} - {content}");
            }

            try
            {
                var json = JsonDocument.Parse(content);

                if (!json.RootElement.TryGetProperty("ask", out var askElement))
                    throw new Exception("El campo 'ask' no se encuentra en la respuesta.");

                return askElement.GetDecimal();
            }
            catch (JsonException)
            {
                throw new Exception($"Respuesta no es JSON válido: {content}");
            }
        }

    }
}
