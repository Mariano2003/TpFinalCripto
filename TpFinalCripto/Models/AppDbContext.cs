using Microsoft.EntityFrameworkCore;

namespace TpFinalCripto.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaccion>()
                .HasOne(t => t.Cliente)
                .WithMany(c => c.Transacciones)
                .HasForeignKey(t => t.ClienteId);

            modelBuilder.Entity<Transaccion>()
                .Property(t => t.CryptoAmount)
                .HasPrecision(18, 8);

            modelBuilder.Entity<Transaccion>()
                .Property(t => t.Money)
                .HasPrecision(18, 2);
        }
    }
}