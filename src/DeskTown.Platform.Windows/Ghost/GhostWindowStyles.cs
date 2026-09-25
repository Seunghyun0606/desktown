namespace DeskTown.Platform.Windows.Ghost;

public static class GhostWindowStyles
{
    public const nuint Layered = 0x00080000;
    public const nuint Transparent = 0x00000020;
    public const nuint NoActivate = 0x08000000;
    public const nuint ToolWindow = 0x00000080;
    public const nuint Required = Layered | Transparent | NoActivate | ToolWindow;

    public static nuint Apply(nuint original) => original | Required;
    public static nuint AddedByUs(nuint original) => Required & ~original;
    public static nuint RemoveOwned(nuint current, nuint addedByUs) => current & ~addedByUs;
    public static bool HasRequired(nuint current) => (current & Required) == Required;
}
