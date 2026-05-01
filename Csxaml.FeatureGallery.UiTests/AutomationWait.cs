using System.Diagnostics;

namespace Csxaml.FeatureGallery.UiTests;

internal static class AutomationWait
{
    public static T Until<T>(Func<T?> probe, TimeSpan timeout, string description)
        where T : class
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            var value = probe();
            if (value is not null)
            {
                return value;
            }

            Thread.Sleep(100);
        }

        Assert.Fail($"Timed out waiting for {description}.");
        throw new UnreachableException();
    }

    public static bool UntilTrue(Func<bool> probe, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (probe())
            {
                return true;
            }

            Thread.Sleep(100);
        }

        return false;
    }
}
