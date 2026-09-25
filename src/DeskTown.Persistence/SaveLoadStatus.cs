namespace DeskTown.Persistence;

public enum SaveLoadStatus
{
    NoSave,
    Loaded,
    RecoveredFromBackup,
    Migrated
}
