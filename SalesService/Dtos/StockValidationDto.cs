// SalesService/Dtos/StockValidationDto.cs

using System.Text.Json.Serialization;

namespace SalesService.Dtos
{
    public class StockValidationDto
    {
        [JsonPropertyName("produtoId")]
        public int ProdutoId { get; set; }

        // Usamos esta propriedade para checar se a quantidade é suficiente
        [JsonPropertyName("quantidadeEmEstoque")]
        public int QuantidadeEmEstoque { get; set; } 

        [JsonPropertyName("preco")]
        public decimal PrecoUnitario { get; set; }

        [JsonPropertyName("nome")]
        public string NomeProduto { get; set; } = string.Empty;
    }
}