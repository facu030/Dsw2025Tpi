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

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(15);

        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BillingAddress).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(15,2)");

            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).IsRequired().HasDefaultValue(5);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(15,2)");

            entity.HasOne(oi => oi.Order)
                 .WithMany(o => o.OrderItems)
                 .HasForeignKey(oi => oi.OrderId);
            entity.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);
        });
    }
       
}

