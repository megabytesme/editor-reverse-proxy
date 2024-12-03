using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Linq;

namespace EditorReverseProxy.Tests
{
    public class ProxyControllerTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private ConcurrentDictionary<string, string> _services;

        [SetUp]
        public void SetUp()
        {
            _services = new ConcurrentDictionary<string, string>();
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(_services);
                    });
                });
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task Register_Service_Returns_OK()
        {
            // Arrange
            var registrationData = new { ServiceName = "test-service", ServiceUrl = "http://localhost/test-service" };
            var content = new StringContent(JsonConvert.SerializeObject(registrationData), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/proxy/register", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
