namespace DeskTown.Persistence;

/// <summary>Future-version files are left untouched and must never be overwritten.</summary>
public sealed class UnsupportedSaveSchemaException : InvalidDataException
{
    public UnsupportedSaveSchemaException(int version)
        : base($"Unsupported save schema version {version}.") => Version = version;

    public int Version { get; }
}
