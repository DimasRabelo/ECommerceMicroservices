// StockService.IntegrationTests/StockUpdateTests.cs (CORREÇÃO DE COMPARTILHAMENTO)

using Xunit;
using Microsoft.EntityFrameworkCore;
using StockService.Data;
using StockService.Models;
using System;
using System.Linq; // Necessário para .Any() ou outras operações de linq

public class StockUpdateTests
{
    private const int TestProductId = 10;
    private const int InitialStock = 20;
    private const int QuantitySold = 4;

    // ⬅️ MÉTODO AGORA ACEITA UM NOME PARA REUTILIZAÇÃO
    private StockContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<StockContext>()
            .UseInMemoryDatabase(databaseName: databaseName) // Usa o nome fixo
            .Options;
        
        var context = new StockContext(options);
        return context;
    }

    [Fact]
    public async Task UpdateStock_ShouldReduceQuantity_WhenUsingAsTracking()
    {
        // ⬅️ CRUCIAL: Nome fixo do DB para que os contextos vejam os mesmos dados
        var dbName = "Shared_TestDB_Reduction"; 
        
        // 0. SEEDING (Inicializa o BD com o produto)
        // Usamos um bloco para inserir o dado inicial (20)
        await using (var seedContext = CreateInMemoryContext(dbName)) 
        {
            seedContext.Database.EnsureDeleted(); // Limpa o BD em memória de execuções anteriores
            seedContext.Produtos.Add(new Produto 
            { 
                Id = TestProductId, 
                Nome = "Produto Teste", 
                QuantidadeEmEstoque = InitialStock, 
                Preco = 10.0m, 
                Descricao = "Teste" 
            });
            await seedContext.SaveChangesAsync();
        }

        // 1. ACT (Executar a Lógica de Redução - Constrói o estado 16)
        await using (var actContext = CreateInMemoryContext(dbName)) // ⬅️ Usa o mesmo nome
        {
            var produto = await actContext.Produtos
                .AsTracking() // O rastreamento está ativo
                .FirstOrDefaultAsync(p => p.Id == TestProductId);

            // Confirma que o valor lido é o inicial (20)
            Assert.Equal(InitialStock, produto.QuantidadeEmEstoque); 

            // Executar a redução e persistir:
            if (produto.QuantidadeEmEstoque >= QuantitySold)
            {
                produto.QuantidadeEmEstoque -= QuantitySold; // 20 - 4 = 16
                await actContext.SaveChangesAsync(); // ⬅️ Persiste 16 no BD Compartilhado
            }
        }
        
        // 2. ASSERT (Verificar o Resultado - Lê o estado 16)
        await using (var assertContext = CreateInMemoryContext(dbName)) // ⬅️ Abre o mesmo BD
        {
            var produtoFinal = await assertContext.Produtos.FirstOrDefaultAsync(p => p.Id == TestProductId);

            // O teste deve garantir que o valor persistido é 16
            Assert.NotNull(produtoFinal);
            // ⬅️ VERIFICAÇÃO FINAL
            Assert.Equal(InitialStock - QuantitySold, produtoFinal.QuantidadeEmEstoque); // Esperado: 16
        }
    }
}