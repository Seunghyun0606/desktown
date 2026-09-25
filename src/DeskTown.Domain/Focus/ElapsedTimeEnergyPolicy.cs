namespace DeskTown.Domain.Focus;

/// <summary>
/// Prototype v0.1 policy: every counted, non-suspended second is retained and
/// every complete minute is presented as one Focus unit.
/// </summary>
public sealed class ElapsedTimeEnergyPolicy : IEnergyPolicy
{
    public FocusEnergy CalculateTotal(SessionLedger ledger)
    {
        ArgumentNullException.ThrowIfNull(ledger);

        return FocusEnergy.FromCountedDuration(ledger.TotalCountedDuration);
    }
}
