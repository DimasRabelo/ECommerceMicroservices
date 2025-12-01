// StockService/Controllers/ProdutosController.cs (COMPLETO)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockService.Data;
using StockService.Models;

namespace StockService.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly StockContext _context;

        public ProdutosController(StockContext context)
        {
            _context = context;
        }

        // 1. Funcionalidade: Cadastro de Produtos (POST /api/Produtos)
        [HttpPost]
        public async Task<ActionResult<Produto>> CadastrarProduto(Produto produto)
        {
            if (produto == null || string.IsNullOrWhiteSpace(produto.Nome))
            {
                return BadRequest("O produto deve ter um nome válido.");
            }

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            // Usamos ConsultarPorId aqui para resolver a referência
            return CreatedAtAction(nameof(ConsultarProdutoPorId), new { id = produto.Id }, produto);
        }

        // 2. Funcionalidade: Consulta de Todos os Produtos (GET /api/Produtos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> ConsultarProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        // 3. Funcionalidade: Consulta de Produto por ID (GET /api/Produtos/{id})
        // ⬅️ CORREÇÃO: Endpoint que o SalesService precisa para validar o estoque!
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> ConsultarProdutoPorId(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                return NotFound(); // Retorna 404 se não encontrar
            }

            return produto;
        }
    }
}