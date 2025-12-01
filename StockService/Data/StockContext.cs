using Microsoft.EntityFrameworkCore;
using StockService.Models;

namespace StockService.Data 
{
    public class StockContext : DbContext
    {
        public StockContext(DbContextOptions<StockContext> options) : base(options)
        {
        }
        public DbSet<Produto> Produtos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define a coluna 'Preco' como DECIMAL(18, 2), ideal para moeda
            modelBuilder.Entity<Produto>()
                .Property(p => p.Preco)
                .HasPrecision(18, 2); 
        }
    }
}