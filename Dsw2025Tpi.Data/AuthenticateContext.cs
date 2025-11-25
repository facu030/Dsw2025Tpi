using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Data
{
    public class AuthenticateContext : IdentityDbContext
    {
        public AuthenticateContext(DbContextOptions<AuthenticateContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>(b =>{ b.ToTable("Usuarios"); });
            builder.Entity<IdentityRole>(b => { b.ToTable("Roles"); });
            builder.Entity<IdentityUserRole<string>>(b => { b.ToTable("UsuarioRoles"); });
            builder.Entity<IdentityUserClaim<string>>(b => { b.ToTable("UsuarioClaims"); });
            builder.Entity<IdentityUserLogin<string>>(b => { b.ToTable("UsuarioLogins"); });
            builder.Entity<IdentityRoleClaim<string>>(b => { b.ToTable("RolClaims"); });
            builder.Entity<IdentityUserToken<string>>(b => { b.ToTable("UsuarioTokens"); } );

            builder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");

                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
