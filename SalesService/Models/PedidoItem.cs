// SalesService/Models/PedidoItem.cs (CORRIGIDO)
using System.Text.Json.Serialization; // ⬅️ NOVO: Necessário para [JsonIgnore]

namespace SalesService.Models
{
    public class PedidoItem
    {
        public int Id { get; set; }
        
        public int ProdutoId { get; set; } 
        
        public  string NomeProduto { get; set; } = null!;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }

        public int PedidoId { get; set; }
        
        // CORREÇÃO: Adicionamos [JsonIgnore] para quebrar o loop:
        // O serializador não tentará serializar a propriedade Pedido, que
        // aponta de volta, resolvendo o erro de ciclo.
        [JsonIgnore] 
        public Pedido Pedido { get; set; } = null!; 
    }
}