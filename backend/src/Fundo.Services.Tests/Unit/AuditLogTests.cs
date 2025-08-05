using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fundo.Applications.WebApi;
using Fundo.Applications.WebApi.Controllers;
using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Fundo.Services.Tests.Unit
{
    public class AuditLogTests
    {
        private LoanContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LoanContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new LoanContext(options);
            return context;
        }

        [Fact]
        public async Task CreateLoan_ShouldCreateAuditLog()
        {
            using var context = GetInMemoryDbContext();
            var controller = new LoanManagementController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "testuser")
                        }, "someAuthTypeName"))
                    }
                }
            };
            var loan = new Loan { Amount = 1000, CurrentBalance = 1000, ApplicantName = "Test", Status = "active" };

            await controller.CreateLoan(loan);

            Assert.Single(context.AuditLogs);
            var auditLog = context.AuditLogs.First();
            Assert.Equal("Loan created", auditLog.Action);
            Assert.Equal("testuser", auditLog.UserId);
        }

        [Fact]
        public async Task MakePayment_ShouldCreateAuditLog()
        {
            using var context = GetInMemoryDbContext();
            var loan = new Loan { Id = 1, Amount = 1000, CurrentBalance = 1000, ApplicantName = "Test", Status = "active" };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var controller = new LoanManagementController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "testuser")
                        }, "someAuthTypeName"))
                    }
                }
            };

            await controller.MakePayment(1, 500);

            Assert.Single(context.AuditLogs);
            var auditLog = context.AuditLogs.First();
            Assert.Equal("Payment of 500 made", auditLog.Action);
            Assert.Equal("testuser", auditLog.UserId);
        }
    }
}