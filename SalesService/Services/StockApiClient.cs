// SalesService/Services/StockApiClient.cs

using SalesService.Dtos;
using System.Text.Json;

namespace SalesService.Services
{
    public class StockApiClient
    {
        private readonly HttpClient _httpClient;

        // O HttpClient é injetado, configurado no Program.cs
        public StockApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // CORREÇÃO: Método renomeado para 'ConsultarEstoqueAsync' e 'await' adicionado 
        // para resolver o warning CS1998 e o erro CS1061.
        public async Task<StockValidationDto?> ConsultarEstoqueAsync(int produtoId)
        {
            // Rota: GET /api/Produtos/{produtoId}
            var response = await _httpClient.GetAsync($"api/Produtos/{produtoId}"); 

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Desserializa a resposta, ignorando a caixa (case insensitive)
                var estoqueDto = JsonSerializer.Deserialize<StockValidationDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                return estoqueDto;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Produto não existe no Estoque
                return null;
            }
            
            // Retorna null em caso de erro de comunicação
            return null;
        }
    }
}