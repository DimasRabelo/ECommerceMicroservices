namespace StockService.Models
{
    public class Produto
    {
        public int Id { get; set; }
        
        // Adicionando '?' para indicar que podem ser nulos,
        // OU usando o modificador 'required' (melhor em C# 11+)
        // Vamos usar o 'required' para garantir que os campos sejam preenchidos
        public required string Nome { get; set; }
        public required string Descricao { get; set; }
        
        public decimal Preco { get; set; }
        public int QuantidadeEmEstoque { get; set; }
    }
}