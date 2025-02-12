using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Sentry.AspNetCore.Tests;

public abstract class SentrySdkTestFixture : IDisposable
{
    public TestServer TestServer { get; set; }

    public HttpClient HttpClient { get; set; }
    public IServiceProvider ServiceProvider { get; set; }

    public Action<IServiceCollection> ConfigureServices { get; set; }
    public Action<IApplicationBuilder> ConfigureApp { get; set; }
    public Action<IWebHostBuilder> ConfigureWebHost { get; set; }

    public LastExceptionFilter LastExceptionFilter { get; private set; }

    public IReadOnlyCollection<RequestHandler> Handlers { get; set; } = new[]
    {
        new RequestHandler
        {
            Path = "/",
            Response = "home"
        },
        new RequestHandler
        {
            Path = "/throw",
            Handler = _ => throw new Exception("test error")
        }
    };

    protected virtual void Build(string environment = default)
    {
        var builder = new WebHostBuilder();

        if (!string.IsNullOrWhiteSpace(environment))
        {
            builder.UseEnvironment(environment);
        }

        _ = builder.ConfigureServices(s =>
        {
            var lastException = new LastExceptionFilter();
            _ = s.AddSingleton<IStartupFilter>(lastException);
            _ = s.AddSingleton(lastException);

            ConfigureServices?.Invoke(s);
        });
        _ = builder.Configure(app =>
        {
            ConfigureApp?.Invoke(app);
            _ = app.Use(async (context, next) =>
            {
                var handler = Handlers.FirstOrDefault(p => p.Path == context.Request.Path);

                await (handler?.Handler(context) ?? next());
            });
        });

        ConfigureWebHost?.Invoke(builder);
        ConfigureBuilder(builder);

        TestServer = new TestServer(builder);
        HttpClient = TestServer.CreateClient();
        ServiceProvider = TestServer.Host.Services;
        LastExceptionFilter = ServiceProvider.GetRequiredService<LastExceptionFilter>();
    }

    protected virtual void ConfigureBuilder(WebHostBuilder builder)
    {

    }

    public void Dispose()
    {
        SentrySdk.Close();
    }
}
