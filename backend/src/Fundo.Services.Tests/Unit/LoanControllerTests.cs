using Xunit;
using Microsoft.EntityFrameworkCore;
using Fundo.Applications.WebApi.Controllers;
using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Fundo.Applications.WebApi;
using System;

namespace Fundo.Services.Tests.Unit
{
    public class LoanControllerTests
    {
        private LoanContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LoanContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new LoanContext(options);
        }

        [Fact]
        public async Task CreateLoan_ValidLoan_ReturnsCreatedAtAction()
        {
            using var context = GetInMemoryDbContext();
            var controller = new LoanManagementController(context);
            var loan = new Loan { Amount = 1000, CurrentBalance = 1000, ApplicantName = "Test", Status = "active" };

            var result = await controller.CreateLoan(loan);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetLoan", createdResult.ActionName);
            Assert.Single(context.Loans);
            var savedLoan = await context.Loans.FirstAsync();
            Assert.Equal(loan.Amount, savedLoan.Amount);
            Assert.Equal(loan.CurrentBalance, savedLoan.CurrentBalance);
            Assert.Equal(loan.ApplicantName, savedLoan.ApplicantName);
            Assert.Equal(loan.Status, savedLoan.Status);
        }

        [Fact]
        public async Task GetLoan_ExistingId_ReturnsOk()
        {
            using var context = GetInMemoryDbContext();
            var loan = new Loan { Amount = 1000, CurrentBalance = 1000, ApplicantName = "Test", Status = "active" };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var controller = new LoanManagementController(context);

            var result = await controller.GetLoan(loan.Id);

            Assert.NotNull(result.Value);
            var returnedLoan = result.Value;
            Assert.Equal(loan.Id, returnedLoan.Id);
            Assert.Equal(loan.Amount, returnedLoan.Amount);
            Assert.Equal(loan.CurrentBalance, returnedLoan.CurrentBalance);
            Assert.Equal(loan.ApplicantName, returnedLoan.ApplicantName);
            Assert.Equal(loan.Status, returnedLoan.Status);
        }

        [Fact]
        public async Task GetLoan_NonExistingId_ReturnsNotFound()
        {
            using var context = GetInMemoryDbContext();
            var controller = new LoanManagementController(context);

            var result = await controller.GetLoan(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task MakePayment_ValidAmount_UpdatesBalance()
        {
            using var context = GetInMemoryDbContext();
            var loan = new Loan { Amount = 1000, CurrentBalance = 1000, ApplicantName = "Test", Status = "active" };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var controller = new LoanManagementController(context);

            var result = await controller.MakePayment(loan.Id, 500);

            Assert.IsType<OkResult>(result);
            var updatedLoan = await context.Loans.FindAsync(loan.Id);
            Assert.Equal(500, updatedLoan.CurrentBalance);
            Assert.Equal("active", updatedLoan.Status); // Should still be active since balance > 0
        }
    }
}