using Microsoft.AspNetCore.Http;

namespace Sentry.AspNetCore.Tests;

public class RequestHandler
{
    public string Path { get; set; }

    private Func<HttpContext, Task> _handler;
    public Func<HttpContext, Task> Handler
    {
        get => _handler ?? (c => c.Response.WriteAsync(Response));
        set => _handler = value;
    }

    public string Response { get; set; }
}
