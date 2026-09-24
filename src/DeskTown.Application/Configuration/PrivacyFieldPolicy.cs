namespace DeskTown.Application.Configuration;

/// <summary>
/// Prevents accidental introduction of fields forbidden by the v0.1 privacy
/// contract. This guards structured field names; adapters must still avoid
/// placing sensitive values in otherwise safe fields.
/// </summary>
public static class PrivacyFieldPolicy
{
    private static readonly HashSet<string> ForbiddenNames = new(StringComparer.Ordinal)
    {
        "window_title",
        "browser_url",
        "typed_text",
        "keystroke",
        "password",
        "form_value",
        "screenshot",
        "document_content",
        "file_content",
        "native_handle",
        "hwnd",
        "process_id",
        "pid"
    };

    public static bool IsAllowed(string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        var normalized = fieldName.Trim().ToLowerInvariant();
        return !ForbiddenNames.Contains(normalized);
    }

    public static void EnsureAllowed(IEnumerable<string> fieldNames)
    {
        var forbidden = fieldNames.Where(field => !IsAllowed(field)).ToArray();
        if (forbidden.Length > 0)
        {
            throw new InvalidOperationException(
                $"Structured log fields violate the privacy contract: {string.Join(", ", forbidden)}");
        }
    }
}
