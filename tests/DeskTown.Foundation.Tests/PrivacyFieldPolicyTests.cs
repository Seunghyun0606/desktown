using DeskTown.Application.Configuration;

namespace DeskTown.Foundation.Tests;

public sealed class PrivacyFieldPolicyTests
{
    public static TheoryData<string> ForbiddenFields => new()
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

    [Theory]
    [MemberData(nameof(ForbiddenFields))]
    public void Forbidden_tracking_fields_are_rejected(string fieldName)
    {
        Assert.False(PrivacyFieldPolicy.IsAllowed(fieldName));
    }

    [Theory]
    [InlineData("event_name")]
    [InlineData("process_name")]
    [InlineData("active_duration_seconds")]
    [InlineData("idle_duration_seconds")]
    [InlineData("session_id")]
    public void Contract_fields_are_allowed(string fieldName)
    {
        Assert.True(PrivacyFieldPolicy.IsAllowed(fieldName));
    }

    [Fact]
    public void Ensure_allowed_reports_every_forbidden_field()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PrivacyFieldPolicy.EnsureAllowed(["session_id", "window_title", "browser_url"]));

        Assert.Contains("window_title", exception.Message, StringComparison.Ordinal);
        Assert.Contains("browser_url", exception.Message, StringComparison.Ordinal);
    }
}
