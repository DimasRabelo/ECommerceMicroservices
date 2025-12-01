// SalesService/Models/Pedido.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesService.Models
{
    // Define o Enum de Status (Corrige o Erro CS0103)
    public enum StatusPedido
    {
        Pendente = 1,
        Confirmado = 2,
        Cancelado = 3
    }
    
    public class Pedido
    {
        public int Id { get; set; }
        
        // CORREÇÃO: Propriedade ClienteId adicionada (Corrige o Erro CS0117)
        // O valor será preenchido pelo token JWT no Controller
        public string ClienteId { get; set; } = null!;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        // Tipo do Status alterado de string para o Enum robusto
        public StatusPedido Status { get; set; } = StatusPedido.Pendente; 
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        // Relação 1-para-M: Um Pedido tem muitos Itens do Pedido
        public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();
    }
}