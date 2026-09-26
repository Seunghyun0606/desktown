namespace DeskTown.Application.Activity;

/// <summary>The same privacy and length rule applies before aggregation and save.</summary>
public static class ProcessNamePolicy
{
    public const int MaxLength = 128;

    public static bool IsPersistable(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= MaxLength &&
        !name.Any(c => char.IsControl(c) || c is '/' or '\\' or ':');

    public static string Normalize(string? name) =>
        IsPersistable(name) ? name!.Trim() : ActivitySample.UnknownProcessName;
}
