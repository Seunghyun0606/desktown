namespace DeskTown.Domain.Focus;

public interface IEnergyPolicy
{
    FocusEnergy CalculateTotal(SessionLedger ledger);
}
