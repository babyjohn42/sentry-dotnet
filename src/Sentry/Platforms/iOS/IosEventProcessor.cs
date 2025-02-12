using Sentry.Extensibility;
using Sentry.iOS.Extensions;

namespace Sentry.iOS;

internal class IosEventProcessor : ISentryEventProcessor, IDisposable
{
    private readonly SentryCocoaOptions _options;

    public IosEventProcessor(SentryCocoaOptions options)
    {
        _options = options;
    }

    public SentryEvent Process(SentryEvent @event)
    {
        // Get a temp event from the Cocoa SDK
        using var tempEvent = GetTempEvent();

        // Now we'll copy the context info into our own, leveraging the fact that the JSON
        // serialization is compatible, since both are designed to send the same data to Sentry.
        var json = tempEvent.Context?.ToJsonString();
        if (json != null)
        {
            var jsonDoc = JsonDocument.Parse(json);
            var contexts = Contexts.FromJson(jsonDoc.RootElement);
            contexts.CopyTo(@event.Contexts);
        }

        return @event;
    }

    private static SentryCocoa.SentryEvent GetTempEvent()
    {
        // This will populate an event with all of the information we need, without actually capturing that event.
        var @event = new SentryCocoa.SentryEvent();
        SentryCocoa.SentrySdk.ConfigureScope(scope =>
        {
            scope.ApplyToEvent(@event, 0);
        });

        return @event;
    }

    public void Dispose()
    {
    }
}
