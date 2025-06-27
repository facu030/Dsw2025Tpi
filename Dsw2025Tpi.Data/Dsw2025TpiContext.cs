using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext: DbContext
{
    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(eb =>
        {
            eb.ToTable("Productos");
            eb.Property(p => p.Sku).HasMaxLength(20).IsRequired();
            eb.Property(p => p.InternalCode).HasMaxLength(20).IsRequired();
            eb.Property(p => p.Name).HasMaxLength(20).IsRequired();
            eb.Property(P => P.Description).HasMaxLength(100);
            eb.Property(p => p.CurrentUnitPrice).HasPrecision(15, 2).IsRequired();
            eb.Property(p => p.StockQuantity).HasMaxLength(20).IsRequired();

        });
    }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;",
                b => b.MigrationsAssembly("Dsw2025Tpi.Data")); // ← Ensamblado explícito
        }
       }

}

