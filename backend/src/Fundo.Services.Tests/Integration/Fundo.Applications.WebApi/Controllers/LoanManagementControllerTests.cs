using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Fundo.Applications.WebApi.Controllers;
using Fundo.Applications.WebApi.Models;
using Fundo.Applications.WebApi;
using System;
using Microsoft.AspNetCore.Mvc;

namespace Fundo.Services.Tests.Integration
{
    public class LoanManagementControllerTests : IClassFixture<WebApplicationFactory<Fundo.Applications.WebApi.Startup>>
    {
        private readonly WebApplicationFactory<Applications.WebApi.Startup> _factory;
        private readonly HttpClient _client;

        public LoanManagementControllerTests(WebApplicationFactory<Fundo.Applications.WebApi.Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private LoanContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LoanContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new LoanContext(options);
        }

        private async Task<HttpClient> GetAuthenticatedClientAsync()
        {
            var client = _factory.CreateClient();
            var loginModel = new LoginRequest { Username = "admin", Password = "password" };
            var response = await client.PostAsJsonAsync("/auth/login", loginModel);
            response.EnsureSuccessStatusCode();
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);
            return client;
        }

        [Fact]
        public async Task GetBalances_ShouldReturnExpectedResult()
        {
            var client = await GetAuthenticatedClientAsync();
            var response = await client.GetAsync("/loans"); 

            response.EnsureSuccessStatusCode(); 
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateLoan_ShouldAddLoan()
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

            var result = await controller.CreateLoan(loan);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetLoan", createdResult.ActionName);
            Assert.Single(context.Loans); 
        }

        [Fact]
        public async Task MakePayment_ValidAmount_UpdatesBalance()
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

            var result = await controller.MakePayment(1, 500);

            Assert.IsType<OkResult>(result);
            var updatedLoan = await context.Loans.FindAsync(1);
            Assert.Equal(500, updatedLoan.CurrentBalance);
        }
    }
}