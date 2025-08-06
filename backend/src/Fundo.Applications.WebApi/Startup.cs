using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Fundo.Applications.WebApi.Models;
using Fundo.Applications.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

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
            
            // Add JWT authentication
            var jwtSecretKey = _configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast256BitsLongForSecurity!@#$%^&*()";
            var key = Encoding.ASCII.GetBytes(jwtSecretKey);
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
            
            services.AddAuthorization();
            
            // Register services
            services.AddScoped<JwtService>();
            services.AddScoped<AuthService>();
            
            services.AddControllers();
            
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200") // Angular dev server
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, LoanContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors(builder => builder
                .WithOrigins("http://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            
            app.UseRouting();
            
            // Add authentication and authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            
            // Seed data
            SeedData(context);
            SeedUsers(context);
        }
        private void SeedData(LoanContext context)
        {
            context.Loans.RemoveRange(context.Loans);
            context.SaveChanges();

            var initialLoans = new List<Loan>
            {
                new Loan { Amount = 25000, CurrentBalance = 18750, ApplicantName = "John Doe", Status = "active" },
                new Loan { Amount = 15000, CurrentBalance = 0, ApplicantName = "Jane Smith", Status = "paid" },
                new Loan { Amount = 50000, CurrentBalance = 32500, ApplicantName = "Robert Johnson", Status = "active" },
                new Loan { Amount = 10000, CurrentBalance = 0, ApplicantName = "Emily Williams", Status = "paid" },
                new Loan { Amount = 75000, CurrentBalance = 72000, ApplicantName = "Michael Brown", Status = "active" }
            };

            context.Loans.AddRange(initialLoans);

            var random = new Random();
            var firstNames = new[] { "Olivia", "Liam", "Emma", "Noah", "Ava", "Oliver", "Sophia", "Elijah", "Isabella", "James" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            var statuses = new[] { "active", "paid", "defaulted" };

            for (int i = 0; i < 45; i++)
            {
                var applicantName = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";
                var amount = random.Next(5000, 100001);
                var status = statuses[random.Next(statuses.Length)];
                var currentBalance = status == "paid" ? 0 : random.Next(0, amount);

                context.Loans.Add(new Loan
                {
                    Amount = amount,
                    CurrentBalance = currentBalance,
                    ApplicantName = applicantName,
                    Status = status
                });
            }

            context.SaveChanges();
        }
        
        private void SeedUsers(LoanContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User 
                    { 
                        Username = "admin",
                        Email = "admin@fundoloan.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        FirstName = "System",
                        LastName = "Administrator",
                        Role = UserRoles.Admin,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new User 
                    { 
                        Username = "manager",
                        Email = "manager@fundoloan.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                        FirstName = "Loan",
                        LastName = "Manager",
                        Role = UserRoles.Manager,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new User 
                    { 
                        Username = "user",
                        Email = "user@fundoloan.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                        FirstName = "Regular",
                        LastName = "User",
                        Role = UserRoles.User,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
