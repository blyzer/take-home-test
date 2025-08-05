using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Fundo.Services.Tests.Integration
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Fundo.Applications.WebApi.Startup>>
    {
        private readonly WebApplicationFactory<Fundo.Applications.WebApi.Startup> _factory;
        private readonly HttpClient _client;

        public AuthControllerTests(WebApplicationFactory<Fundo.Applications.WebApi.Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsJwtToken()
        {
            var loginModel = new LoginRequest { Username = "admin", Password = "password" };
            var response = await _client.PostAsJsonAsync("/auth/login", loginModel);

            response.EnsureSuccessStatusCode();

            var loginResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            Assert.NotNull(loginResponse.Token);
        }

        [Fact]
        public async Task GetLoans_WithoutAuthentication_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/loans");

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}