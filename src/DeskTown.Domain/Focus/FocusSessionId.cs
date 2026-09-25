namespace DeskTown.Domain.Focus;

public readonly record struct FocusSessionId
{
    private FocusSessionId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static FocusSessionId Create()
    {
        return new FocusSessionId(Guid.NewGuid());
    }

    public static FocusSessionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A focus session ID cannot be empty.", nameof(value));
        }

        return new FocusSessionId(value);
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
