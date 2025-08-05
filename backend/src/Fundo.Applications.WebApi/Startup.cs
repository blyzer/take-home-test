using System.Linq;
using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fundo.Applications.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LoanContext>(options =>
            options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")));
            services.AddControllers();
            
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200") // Angular dev server
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, LoanContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors("AllowAngularApp");
            
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            SeedData(context);
        }
        private void SeedData(LoanContext context)
        {
            if (!context.Loans.Any())
            {
                context.Loans.AddRange(
                    new Loan { Amount = 25000, CurrentBalance = 18750, ApplicantName = "John Doe", Status = "active" },
                    new Loan { Amount = 15000, CurrentBalance = 0, ApplicantName = "Jane Smith", Status = "paid" },
                    new Loan { Amount = 50000, CurrentBalance = 32500, ApplicantName = "Robert Johnson", Status = "active" },
                    new Loan { Amount = 10000, CurrentBalance = 0, ApplicantName = "Emily Williams", Status = "paid" },
                    new Loan { Amount = 75000, CurrentBalance = 72000, ApplicantName = "Michael Brown", Status = "active" }
                );
                context.SaveChanges();
            }
        }
    }
}
