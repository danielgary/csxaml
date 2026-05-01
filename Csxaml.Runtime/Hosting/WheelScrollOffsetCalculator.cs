namespace Csxaml.Runtime;

internal static class WheelScrollOffsetCalculator
{
    private const double DefaultPixelsPerWheelDetent = 48;
    private const double WheelDetentDelta = 120;

    public static double CalculateNextOffset(
        double currentOffset,
        double scrollableHeight,
        int wheelDelta)
    {
        if (wheelDelta == 0 || scrollableHeight <= 0)
        {
            return Clamp(currentOffset, scrollableHeight);
        }

        var detents = wheelDelta / WheelDetentDelta;
        var nextOffset = currentOffset - detents * DefaultPixelsPerWheelDetent;
        return Clamp(nextOffset, scrollableHeight);
    }

    private static double Clamp(double offset, double scrollableHeight)
    {
        if (offset < 0)
        {
            return 0;
        }

        if (offset > scrollableHeight)
        {
            return scrollableHeight;
        }

        return offset;
    }
}
