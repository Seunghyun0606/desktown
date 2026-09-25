namespace DeskTown.Domain.Focus;

/// <summary>
/// Focus progression retained at TimeSpan tick precision. Whole seconds and
/// minutes are presentation views, so composing short sessions never loses time.
/// </summary>
public readonly record struct FocusEnergy : IComparable<FocusEnergy>
{
    private readonly long _countedTicks;

    private FocusEnergy(long countedTicks)
    {
        _countedTicks = countedTicks;
    }

    public static FocusEnergy Zero => default;

    public TimeSpan CountedDuration => TimeSpan.FromTicks(_countedTicks);

    public long CountedSeconds => _countedTicks / TimeSpan.TicksPerSecond;

    public long DisplayedWholeMinutes => CountedSeconds / 60;

    public static FocusEnergy FromCountedSeconds(long countedSeconds)
    {
        if (countedSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(countedSeconds),
                countedSeconds,
                "Focus Energy seconds cannot be negative.");
        }

        return new FocusEnergy(checked(countedSeconds * TimeSpan.TicksPerSecond));
    }

    public static FocusEnergy FromCountedDuration(TimeSpan countedDuration)
    {
        if (countedDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(countedDuration),
                countedDuration,
                "Focus Energy duration cannot be negative.");
        }

        return new FocusEnergy(countedDuration.Ticks);
    }

    public static FocusEnergy operator +(FocusEnergy left, FocusEnergy right)
    {
        return new FocusEnergy(checked(left._countedTicks + right._countedTicks));
    }

    public FocusEnergy DeltaSince(FocusEnergy previouslyAppliedTotal)
    {
        if (previouslyAppliedTotal._countedTicks > _countedTicks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previouslyAppliedTotal),
                "Applied Focus Energy cannot exceed the current total.");
        }

        return new FocusEnergy(_countedTicks - previouslyAppliedTotal._countedTicks);
    }

    public int CompareTo(FocusEnergy other)
    {
        return _countedTicks.CompareTo(other._countedTicks);
    }
}
