using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext : DbContext
{

    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(eb =>
        {
            //eb.ToTable("Products");
            eb.Property(p => p.Sku).HasMaxLength(20).IsRequired();
            eb.Property(p => p.InternalCode).HasMaxLength(20).IsRequired();
            eb.Property(p => p.Name).HasMaxLength(60);
            eb.Property(p => p.Description).HasMaxLength(150);
            eb.Property(p => p.CurrentUnitPrice).HasPrecision(15, 2);
        });

        modelBuilder.Entity<Order>(eb =>
        {
            eb.Property(o => o.Date).IsRequired();
            eb.Property(o => o.ShippingAddress).HasMaxLength(150).IsRequired();
            eb.Property(o => o.BillingAddress).HasMaxLength(150).IsRequired();
            eb.Property(o => o.Notes).HasMaxLength(200);
            eb.Property(o => o.TotalAmount).HasPrecision(15, 2);
        });

        modelBuilder.Entity<OrderItem>(eb =>
        {
            eb.Property(oi => oi.Quantity).IsRequired();
            eb.Property(oi => oi.UnitPrice).HasPrecision(15, 2).IsRequired();
            eb.Property(oi => oi.SubTotal).HasPrecision(15, 2).IsRequired();
        });

        modelBuilder.Entity<Customer>(eb =>
        {
            eb.Property(c => c.EMail).HasMaxLength(50).IsRequired();
            eb.Property(c => c.Name).HasMaxLength(60).IsRequired();
            eb.Property(c => c.PhoneNumber).HasMaxLength(20).IsRequired();
        });

    }


}
