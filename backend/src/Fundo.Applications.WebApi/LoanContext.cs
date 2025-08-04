using Microsoft.EntityFrameworkCore;

namespace Fundo.Applications.WebApi
{
    public class LoanContext : DbContext
    {
        public LoanContext(DbContextOptions<LoanContext> options) : base(options) { }
        public DbSet<Loan> Loans { get; set; }
    }
}