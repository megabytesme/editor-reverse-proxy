using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

[ApiController]
[Route("[controller]")]
public class ProxyController : ControllerBase
{
    private readonly ConcurrentDictionary<string, string> _services;

    public ProxyController(ConcurrentDictionary<string, string> services)
    {
        _services = services;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] ServiceRegistration registration)
    {
        _services[registration.Service] = registration.Url;
        return Ok();
    }

    [HttpGet("services")]
    public IActionResult GetServices()
    {
        return Ok(_services.Keys);
    }
}

public class ServiceRegistration
{
    public string Service { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
