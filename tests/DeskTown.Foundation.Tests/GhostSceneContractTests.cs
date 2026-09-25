namespace DeskTown.Foundation.Tests;

public sealed class GhostSceneContractTests
{
    [Fact]
    public void Ghost_stage_has_no_focusable_or_interactive_nodes()
    {
        var root = FindRoot();
        var stage = File.ReadAllText(Path.Combine(root, "scenes/ghost/GhostStage.tscn"));
        Assert.DoesNotContain("type=\"Control\"", stage, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"Button\"", stage, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"Label\"", stage, StringComparison.Ordinal);

        var host = File.ReadAllText(Path.Combine(root,
            "src/DeskTown.Godot/Display/GhostWindowHost.cs"));
        Assert.DoesNotContain("override void _Input", host, StringComparison.Ordinal);
        Assert.DoesNotContain("override void _UnhandledInput", host, StringComparison.Ordinal);
    }

    [Fact]
    public void Native_ghost_is_hidden_and_transparent_by_default()
    {
        var scene = File.ReadAllText(Path.Combine(FindRoot(),
            "scenes/ghost/GhostWindow.tscn"));
        foreach (var property in new[] { "visible = false", "transparent = true",
                     "transparent_bg = true", "unfocusable = true",
                     "mouse_passthrough = true", "always_on_top = true" })
            Assert.Contains(property, scene, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DeskTown.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
