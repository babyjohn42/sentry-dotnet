using Sentry.Android.Extensions;

namespace Sentry.Android.Callbacks;

internal class BeforeBreadcrumbCallback : JavaObject, Java.SentryOptions.IBeforeBreadcrumbCallback
{
    private readonly Func<Breadcrumb, Breadcrumb?> _beforeBreadcrumb;

    public BeforeBreadcrumbCallback(Func<Breadcrumb, Breadcrumb?> beforeBreadcrumb)
    {
        _beforeBreadcrumb = beforeBreadcrumb;
    }

    public Java.Breadcrumb? Execute(Java.Breadcrumb b, Java.Hint h)
    {
        // Note: Hint is unused due to:
        // https://github.com/getsentry/sentry-dotnet/issues/1469

        var breadcrumb = b.ToBreadcrumb();
        var result = _beforeBreadcrumb.Invoke(breadcrumb);

        if (result == breadcrumb)
        {
            // The result is the same object as was input, and all properties are immutable,
            // so we can return the original Java object for better performance.
            return b;
        }

        return result?.ToJavaBreadcrumb();
    }
}
