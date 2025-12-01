// SalesService/Controllers/PedidosController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Dtos; 
using SalesService.MessageBus; 
using SalesService.Models;
using SalesService.Services;


namespace SalesService.Controllers
{
    // DTOs de entrada para o Controller
    public class PedidoCriacaoDto 
    {
        public List<PedidoItemCriacaoDto> Itens { get; set; } = new();
    }
    public class PedidoItemCriacaoDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
    
    // DTO do Evento de Venda (payload para o RabbitMQ)
    public class VendaConfirmadaEvent 
    {
        public int ProdutoId { get; set; }
        public int QuantidadeVendida { get; set; }
        public string PedidoId { get; set; } = null!;
    }


    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly SalesContext _context;
        private readonly StockApiClient _stockApiClient;
        private readonly IMessageBusClient _messageBusClient;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(
            SalesContext context,
            StockApiClient stockApiClient,
            IMessageBusClient messageBusClient,
            ILogger<PedidosController> logger)
        {
            _context = context;
            _stockApiClient = stockApiClient;
            _messageBusClient = messageBusClient;
            _logger = logger;
        }

        // -------------------------------------------------------------------
        // CRIAÇÃO DE PEDIDOS (POST)
        // -------------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<Pedido>> CriarPedido([FromBody] PedidoCriacaoDto criacaoDto)
        {
            if (!criacaoDto.Itens.Any())
            {
                return BadRequest("O pedido deve conter itens.");
            }
            
            // O uso de User.Identity?.Name requer que o JWT contenha o claim 'name'
            var novoPedido = new Pedido { ClienteId = User.Identity?.Name ?? "ClienteAnonimo" };
            decimal valorTotal = 0;

            // 1. VALIDAÇÃO DE ESTOQUE (Comunicação SÍNCRONA)
            var itensValidados = new List<PedidoItem>();

            foreach (var itemDto in criacaoDto.Itens)
            {
                // Consulta o estoque real via HTTP
                var estoqueDto = await _stockApiClient.ConsultarEstoqueAsync(itemDto.ProdutoId);

                if (estoqueDto == null)
                {
                    return NotFound($"Produto ID {itemDto.ProdutoId} não encontrado ou serviço indisponível.");
                }

                // Checagem de estoque
                if (itemDto.Quantidade <= 0 || estoqueDto.QuantidadeEmEstoque < itemDto.Quantidade)
                {
                    return BadRequest($"Estoque insuficiente para o produto '{estoqueDto.NomeProduto}'. Disponível: {estoqueDto.QuantidadeEmEstoque}");
                }

                // Cria o item do pedido com dados do estoque
                var item = new PedidoItem
                {
                    ProdutoId = itemDto.ProdutoId,
                    NomeProduto = estoqueDto.NomeProduto,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = estoqueDto.PrecoUnitario,
                    Pedido = novoPedido
                };
                itensValidados.Add(item);
                valorTotal += item.PrecoUnitario * item.Quantidade;
            }

            // 2. PERSISTÊNCIA DO PEDIDO
            novoPedido.Itens = itensValidados;
            novoPedido.ValorTotal = valorTotal;
            novoPedido.Status = StatusPedido.Confirmado;
            
            _context.Pedidos.Add(novoPedido);
            await _context.SaveChangesAsync();


            // 3. NOTIFICAÇÃO DE VENDA (Comunicação ASSÍNCRONA via RabbitMQ)
            foreach (var item in novoPedido.Itens)
            {
                var evento = new VendaConfirmadaEvent 
                {
                    ProdutoId = item.ProdutoId,
                    QuantidadeVendida = item.Quantidade,
                    PedidoId = novoPedido.Id.ToString()
                };
                
                _messageBusClient.PublicarMensagem(evento);
            }
            
            _logger.LogInformation($"Pedido {novoPedido.Id} criado e notificação de venda enviada via MessageBus.");

            return CreatedAtAction(nameof(ConsultarPedidoPorId), new { id = novoPedido.Id }, novoPedido);
        }

        // -------------------------------------------------------------------
        // CONSULTA DE PEDIDOS (GET)
        // -------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> ConsultarPedidoPorId(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens) 
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            return pedido;
        }
    }
}