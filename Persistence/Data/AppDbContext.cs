using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public  class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ExchangeRate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasKey(e => new { e.BasecurrencyCode, e.TargetcurrencyCode, e.Date });

                entity.Property(e => e.BasecurrencyCode)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(e => e.TargetcurrencyCode)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(e => e.Rate)
                    .HasColumnType("decimal(18,6)");

                // Index for faster queries
                entity.HasIndex(e => new { e.BasecurrencyCode, e.Date });
                entity.HasIndex(e => new { e.TargetcurrencyCode, e.Date });
            });
        }
    }
}

