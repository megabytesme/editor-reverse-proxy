using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

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
                    services.AddHttpClient();
                });
                webBuilder.Configure(app =>
                {
                    var services = app.ApplicationServices.GetRequiredService<ConcurrentDictionary<string, string>>();
                    var httpClientFactory = app.ApplicationServices.GetRequiredService<IHttpClientFactory>();

                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();

                        endpoints.MapFallback(async context =>
                        {
                            var path = context.Request.Path.ToString().TrimStart('/');
                            var queryString = context.Request.QueryString.ToString();
                            var service = path.Split('/')[0];
                            if (services.TryGetValue(service, out var targetUrl))
                            {
                                var requestPath = path.Substring(service.Length) + queryString;
                                var requestUri = new Uri(new Uri(targetUrl), requestPath);
                                var client = httpClientFactory.CreateClient();

                                var requestMessage = new HttpRequestMessage();
                                var requestMethod = context.Request.Method;
                                if (!HttpMethods.IsGet(requestMethod) &&
                                    !HttpMethods.IsHead(requestMethod) &&
                                    !HttpMethods.IsDelete(requestMethod) &&
                                    !HttpMethods.IsTrace(requestMethod))
                                {
                                    var streamContent = new StreamContent(context.Request.Body);
                                    requestMessage.Content = streamContent;
                                }

                                foreach (var header in context.Request.Headers)
                                {
                                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                                }

                                requestMessage.RequestUri = requestUri;
                                requestMessage.Method = new HttpMethod(requestMethod);
                                var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

                                context.Response.StatusCode = (int)responseMessage.StatusCode;
                                foreach (var header in responseMessage.Headers)
                                {
                                    context.Response.Headers[header.Key] = header.Value.ToArray();
                                }

                                foreach (var header in responseMessage.Content.Headers)
                                {
                                    context.Response.Headers[header.Key] = header.Value.ToArray();
                                }

                                context.Response.ContentType = responseMessage.Content.Headers.ContentType?.ToString();
                                await responseMessage.Content.CopyToAsync(context.Response.Body);
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
