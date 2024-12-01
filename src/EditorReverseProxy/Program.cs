using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddControllers();
                    services.AddSingleton(new ConcurrentDictionary<string, string>());
                });
                webBuilder.Configure(app =>
                {
                    var services = app.ApplicationServices.GetRequiredService<ConcurrentDictionary<string, string>>();

                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();

                        endpoints.MapFallback(async context =>
                        {
                            var path = context.Request.Path.ToString().TrimStart('/');
                            var service = path.Split('/')[0];
                            if (services.TryGetValue(service, out var targetUrl))
                            {
                                var requestPath = path.Substring(service.Length);
                                context.Request.Path = new PathString(requestPath);
                                context.Request.Scheme = "http";
                                context.Request.Host = new HostString(targetUrl);
                                await context.Response.WriteAsync(targetUrl);
                            }
                            else
                            {
                                context.Response.StatusCode = 404;
                                await context.Response.WriteAsync("Service not found");
                            }
                        });
                    });
                });
            });
}
