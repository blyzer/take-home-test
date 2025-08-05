using Microsoft.EntityFrameworkCore;
using Fundo.Applications.WebApi.Models;

namespace Fundo.Applications.WebApi
{
    public class LoanContext : DbContext
    {
        public LoanContext(DbContextOptions<LoanContext> options) : base(options) { }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Loan>(entity =>
            {
                entity.Property(e => e.Amount)
                      .HasPrecision(18, 2); // 18 total digits, 2 after decimal

                entity.Property(e => e.CurrentBalance)
                      .HasPrecision(18, 2); // 18 total digits, 2 after decimal
            });
            
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}