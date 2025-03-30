using Microsoft.EntityFrameworkCore;
using TestDevicon.Server.Models.Entities;

namespace TestDevicon.Server.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Valute> Valutes { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Valute>(entity =>
            {
                entity.HasKey(x => x.Id);
                
                entity.Property(x => x.ValuteCode)
                    .HasMaxLength(7);

                entity.Property(x => x.CharCode)
                    .HasMaxLength(3);

                entity.HasIndex(x => x.ValuteCode);

            });

            modelBuilder.Entity<ExchangeRate>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ValutePrice)
                    .HasPrecision(10, 4);

                entity.HasIndex(x => x.Date);

                entity
                    .HasOne(x => x.Valute)
                    .WithMany(x => x.ExchangeRates)
                    .HasForeignKey(x => x.ValuteId)
                    .OnDelete(DeleteBehavior.Cascade);

            });

        }
    }
}
