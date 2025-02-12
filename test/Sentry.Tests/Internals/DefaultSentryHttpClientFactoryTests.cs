using System.IO.Compression;
using System.Net.Http;
using Sentry.Internal.Http;

namespace Sentry.Tests.Internals;

public class DefaultSentryHttpClientFactoryTests
{
    private class Fixture
    {
        public SentryOptions HttpOptions { get; set; } = new()
        {
            Dsn = ValidDsn
        };

        public static DefaultSentryHttpClientFactory GetSut()
            => new();
    }

    private readonly Fixture _fixture = new();

    [Fact]
    public void Create_Returns_HttpClient()
    {
        var sut = Fixture.GetSut();

        Assert.NotNull(sut.Create(_fixture.HttpOptions));
    }

    [Fact]
    public void Create_CompressionLevelNoCompression_NoGzipRequestBodyHandler()
    {
        _fixture.HttpOptions.RequestBodyCompressionLevel = CompressionLevel.NoCompression;

        var sut = Fixture.GetSut();

        var client = sut.Create(_fixture.HttpOptions);

        foreach (var handler in client.GetMessageHandlers())
        {
            Assert.IsNotType<GzipRequestBodyHandler>(handler);
        }
    }

    [Theory]
    [InlineData(CompressionLevel.Fastest)]
    [InlineData(CompressionLevel.Optimal)]
    public void Create_CompressionLevelEnabled_ByDefault_IncludesGzipRequestBodyHandler(CompressionLevel level)
    {
        _fixture.HttpOptions.RequestBodyCompressionLevel = level;

        var sut = Fixture.GetSut();

        var client = sut.Create(_fixture.HttpOptions);

        Assert.Contains(client.GetMessageHandlers(), h => h.GetType() == typeof(GzipBufferedRequestBodyHandler));
    }

    [Theory]
    [InlineData(CompressionLevel.Fastest)]
    [InlineData(CompressionLevel.Optimal)]
    public void Create_CompressionLevelEnabled_NonBuffered_IncludesGzipRequestBodyHandler(CompressionLevel level)
    {
        _fixture.HttpOptions.RequestBodyCompressionLevel = level;
        _fixture.HttpOptions.RequestBodyCompressionBuffered = false;
        var sut = Fixture.GetSut();

        var client = sut.Create(_fixture.HttpOptions);

        Assert.Contains(client.GetMessageHandlers(), h => h.GetType() == typeof(GzipRequestBodyHandler));
    }

    [Fact]
    public void Create_RetryAfterHandler_FirstHandler()
    {
        var sut = Fixture.GetSut();

        var client = sut.Create(_fixture.HttpOptions);

        Assert.Equal(typeof(RetryAfterHandler), client.GetMessageHandlers().First().GetType());
    }

    [Fact]
    public void Create_DefaultHeaders_AcceptJson()
    {
        var configureHandlerInvoked = false;
        _fixture.HttpOptions.ConfigureClient = client =>
        {
            Assert.Equal("application/json", client.DefaultRequestHeaders.Accept.ToString());
            configureHandlerInvoked = true;
        };
        var sut = Fixture.GetSut();

        _ = sut.Create(_fixture.HttpOptions);

        Assert.True(configureHandlerInvoked);
    }

    [Fact]
    public void Create_ProvidedCreateHttpClientHandler_ReturnedHandlerUsed()
    {
        var handler = Substitute.For<HttpClientHandler>();
        _fixture.HttpOptions.CreateHttpClientHandler = () => handler;
        var sut = Fixture.GetSut();

        var client = sut.Create(_fixture.HttpOptions);

        Assert.Contains(client.GetMessageHandlers(), h => ReferenceEquals(handler, h));
    }

    [Fact]
    public void Create_NullCreateHttpClientHandler_HttpClientHandlerUsed()
    {
        _fixture.HttpOptions.CreateHttpClientHandler = null;
        var sut = Fixture.GetSut();

        var client = sut.Create(_fixture.HttpOptions);

        Assert.Contains(client.GetMessageHandlers(), h => h.GetType() == typeof(HttpClientHandler));
    }

    [Fact]
    public void Create_NullReturnedCreateHttpClientHandler_HttpClientHandlerUsed()
    {
        _fixture.HttpOptions.CreateHttpClientHandler = () => null;
        var sut = Fixture.GetSut();

        var client = sut.Create(_fixture.HttpOptions);

        Assert.Contains(client.GetMessageHandlers(), h => h.GetType() == typeof(HttpClientHandler));
    }
}
