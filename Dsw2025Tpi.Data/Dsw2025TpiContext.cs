using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext : DbContext
{
    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(eb =>
        {
            eb.ToTable("Products");
            eb.Property(p => p.Sku)
              .HasMaxLength(20)
              .IsRequired();

            eb.Property(p => p.InternalCode);
            eb.Property(p => p.Name)
              .HasMaxLength(60);

            eb.Property(p => p.Description);

            eb.Property(p => p.CurrentUnitPrice)
              .HasPrecision(15, 2)
              .IsRequired();

            eb.Property(p => p.StockQuantity)
              .HasConversion<int>()
              .IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Status).HasDefaultValue(OrderStatus.Pending);
            entity.HasOne(o => o.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(o => o.CustomerId);

            entity.Ignore(o => o.TotalAmount);
        });

        modelBuilder.Entity<OrderItem>(eb =>
        {
            eb.ToTable("OrderItems");
            eb.Property(oi => oi.Quantity)
              .IsRequired();

            eb.Property(oi => oi.UnitPrice)
              .HasPrecision(15, 2)
              .IsRequired();
        });
    }
}