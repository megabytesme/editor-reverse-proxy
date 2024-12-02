using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EditorReverseProxy.Tests
{
    public class ProxyControllerTests
    {
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new WebApplicationFactory<Program>();
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
        }

    }
}
